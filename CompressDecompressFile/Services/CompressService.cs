using CompressDecompressFile.Data;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace CompressDecompressFile.Services
{
    /// <summary>
    /// Сервис сжатия файлов
    /// </summary>
    public class CompressService : ICompressActionsService
    {
        public CompressService(string inputFileName, string outputFileName)
        {
            this.inputFileName = inputFileName;
            this.outputFileName = outputFileName;
        }

        /// <summary>
        /// Путь исходного файла
        /// </summary>
        protected string inputFileName;

        /// <summary>
        /// Путь результирующего файла
        /// </summary>
        protected string outputFileName;

        /// <summary>
        /// Коллекция блоков исходного файла
        /// </summary>
        private BlockingCollection<ByteBlock> readCollection = new BlockingCollection<ByteBlock>();

        /// <summary>
        /// Коллекция обработанных блоков
        /// </summary>
        private BlockingCollection<ByteBlock> writeCollection = new BlockingCollection<ByteBlock>();

        /// <summary>
        /// Идентификатор последнего блока считываемого файла
        /// </summary>
        private int inputLastBlockID;

        /// <summary>
        /// Счетчик обработки блоков файла
        /// </summary>
        private int blockCountCursor = 1;

        /// <summary>
        /// Строка информирования начала работы сервиса
        /// </summary>
        public virtual string StartInfoConst
        {
            get
            {
                return "Выполняется сжатие файла ";
            }
        }

        /// <summary>
        /// Строка информирования успешного завершения сервиса
        /// </summary>
        public virtual string SuccessfullEndInfo
        {
            get
            {
                return "Сжатие файла успешно выполнено. Файл: " + outputFileName;
            }
        }

        /// <summary>
        /// Добавить блок обработанных данных в результирующий файл
        /// </summary>
        /// <param name="outputFile">Результирующий файл</param>
        /// <param name="bytes">Обработанные данные</param>
        protected virtual void AppendProcessedBytesToOutFile(string outputFile, byte[] bytes)
        {
            using (FileStream stream = new FileStream(outputFile, FileMode.Append))
            {
                BitConverter.GetBytes(bytes.Length).CopyTo(bytes, 4);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Получить обработанные данные
        /// </summary>
        /// <param name="inputBlock">Входной блок файла</param>
        /// <returns>Массив обработанных данных</returns>
        protected virtual byte[] GetCompressedBytes(ByteBlock inputBlock)
        {
            byte[] res = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream cs = new GZipStream(memoryStream, GetCurrentCompresMode()))
                    cs.Write(inputBlock.Buffer, 0, inputBlock.Buffer.Length);

                res = memoryStream.ToArray();
            }

            return res;
        }

        /// <summary>
        /// Получить метод обработки файла
        /// </summary>
        /// <returns></returns>
        protected virtual CompressionMode GetCurrentCompresMode()
        {
            return CompressionMode.Compress;
        }

        /// <summary>
        /// Получить максимально допустимую длину считывания из файла
        /// </summary>
        /// <param name="inputFileLenght">Длина входного файла</param>
        /// <param name="currentFileProcPos">Текущая позиция обработки в файле</param>
        /// <returns>Длина считывания из файла</returns>
        protected virtual int GetLengthToRead(long inputFileLenght, long currentFileProcPos)
        {
            int res;

            if (inputFileLenght - currentFileProcPos <= Constants.SizeBlock)
                res = (int)(inputFileLenght - currentFileProcPos);
            else
                res = Constants.SizeBlock;

            return res;
        }

        /// <summary>
        /// Получить блок входного файла
        /// </summary>
        /// <param name="inputFile">Входной файл</param>
        /// <param name="currentPos">Текущая позиция в файле</param>
        /// <param name="bytesReadSize">Длина массива результирующего блока</param>
        /// <returns>Блок входного файла</returns>
        protected virtual byte[] GetFileBlock(string inputFile, long currentPos, int bytesReadSize)
        {
            byte[] res = new byte[bytesReadSize];

            using (FileStream fileToBeCompressed = new FileStream(inputFile, FileMode.Open))
            {
                fileToBeCompressed.Position = currentPos;
                fileToBeCompressed.Read(res, 0, bytesReadSize);
            }

            return res;
        }

        /// <summary>
        /// Запустить обработку сервиса
        /// </summary>
        public void Start(out string errors)
        {
            errors = string.Empty;

            try
            {
                File.Create(outputFileName).Dispose();

                Thread readThread = new Thread(new ThreadStart(Read));
                readThread.Start();

                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    Thread processFileThread = new Thread(new ThreadStart(ProcessFile));
                    processFileThread.Start();
                }

                Thread writeThread = new Thread(new ThreadStart(Write));
                writeThread.Start();
                writeThread.Join();
            }
            catch (Exception ex)
            {
                errors = ex.Message;
            }
        }

        /// <summary>
        /// Обработка входного файла
        /// </summary>
        protected void ProcessFile()
        {
            try
            {
                int currentBlockID = 0;

                while (!readCollection.IsCompleted || readCollection.Count > 0)
                {
                    if (!readCollection.TryTake(out ByteBlock block))
                        continue;

                    ByteBlock currentReadBlock = new ByteBlock(block.ID, GetCompressedBytes(block));
                    currentBlockID = block.ID;
                    bool isItemAdded = false;

                    while (!isItemAdded)
                    {
                        if (blockCountCursor == currentReadBlock.ID && writeCollection.Count < Constants.maxProcessedBlocks)
                        {
                            isItemAdded = writeCollection.TryAdd(currentReadBlock, 100);

                            if (inputLastBlockID == currentBlockID)
                            {
                                readCollection.CompleteAdding();
                                writeCollection.CompleteAdding();
                            }
                            else
                                Interlocked.Increment(ref blockCountCursor);
                        }
                        else
                            Thread.Sleep(Constants.ThreadDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error description: {ex.Message}");
            }
        }

        /// <summary>
        /// Чтение входного файла
        /// </summary>
        private void Read()
        {
            try
            {
                long currentPos = 0;
                long inputFileLenght = GetFileLength(inputFileName);
                int bytesReadLenght;
                byte[] currentFileBlock;
                int blockIdCursor = 0;

                while (currentPos < inputFileLenght)
                {
                    bytesReadLenght = GetLengthToRead(inputFileLenght, currentPos);
                    currentFileBlock = GetFileBlock(inputFileName, currentPos, bytesReadLenght);
                    currentPos += bytesReadLenght;
                    blockIdCursor++;

                    ByteBlock readBlock = new ByteBlock(blockIdCursor, currentFileBlock);

                    while (true)
                    {
                        if (readCollection.Count() < Constants.maxProcessedBlocks)
                        {
                            readCollection.Add(readBlock);
                            break;
                        }
                        else
                            Thread.Sleep(Constants.ThreadDelay);
                    }
                }

                inputLastBlockID = blockIdCursor;
                readCollection.CompleteAdding();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Запись обработанных блоков в результирующий файл
        /// </summary>
        private void Write()
        {
            try
            {
                while (true && !writeCollection.IsCompleted)
                {
                    ByteBlock currentWriteBlock = writeCollection.Take();
                    AppendProcessedBytesToOutFile(outputFileName, currentWriteBlock.Buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Получить длину файла
        /// </summary>
        /// <param name="inputFileName">Входной файл</param>
        /// <returns>Длина входного файла</returns>
        private long GetFileLength(string inputFileName)
        {
            long res = 0;
            using (FileStream fileToBeCompressed = new FileStream(inputFileName, FileMode.Open))
            {
                res = fileToBeCompressed.Length;
            }

            return res;
        }
    }
}