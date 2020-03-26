using CompressDecompressFile.Data;
using System;
using System.IO;
using System.IO.Compression;

namespace CompressDecompressFile.Services
{
    /// <summary>
    /// Сервис распаковки файла
    /// </summary>
    public class DecompressService : CompressService, ICompressActionsService
    {
        public DecompressService(string inputFileName, string outputFileName)
            : base(inputFileName, outputFileName) { }

        /// <summary>
        /// Добавить блок обработанных данных в результирующий файл
        /// </summary>
        /// <param name="outputFile">Результирующий файл</param>
        /// <param name="bytes">Обработанные данные</param>
        protected override void AppendProcessedBytesToOutFile(string outputFile, byte[] bytes)
        {
            using (FileStream stream = new FileStream(outputFileName, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Получить обработанные данные
        /// </summary>
        /// <param name="inputBlock">Входной блок файла</param>
        /// <returns>Массив обработанных данных</returns>
        protected override byte[] GetCompressedBytes(ByteBlock inputBlock)
        {
            byte[] res = null;

            using (MemoryStream memoryStream = new MemoryStream(inputBlock.Buffer))
            {
                int byteLenght = BitConverter.ToInt32(inputBlock.Buffer, inputBlock.Buffer.Length - 4);
                res = new byte[byteLenght];

                using (GZipStream cs = new GZipStream(memoryStream, GetCurrentCompresMode()))
                    cs.Read(res, 0, res.Length);
            }

            return res;
        }

        /// <summary>
        /// Получить метод обработки файла
        /// </summary>
        /// <returns></returns>
        protected override CompressionMode GetCurrentCompresMode()
        {
            return CompressionMode.Decompress;
        }

        /// <summary>
        /// Получить максимально допустимую длину считывания из файла
        /// </summary>
        /// <param name="inputFileLenght">Длина входного файла</param>
        /// <param name="currentFileProcPos">Текущая позиция обработки в файле</param>
        /// <returns>Длина считывания из файла</returns>
        protected override int GetLengthToRead(long inputFileLenght, long currentFileProcPos)
        {
            int res;

            using (FileStream compressedFile = new FileStream(inputFileName, FileMode.Open))
            {
                compressedFile.Position = currentFileProcPos;
                byte[] byteBuffer = new byte[8];
                compressedFile.Read(byteBuffer, 0, byteBuffer.Length);
                res = BitConverter.ToInt32(byteBuffer, 4);
            }

            return res;
        }

        /// <summary>
        /// Строка информирования начала работы сервиса
        /// </summary>
        public override string StartInfoConst
        {
            get
            {
                return "Выполняется распаковка файла ";
            }
        }

        /// <summary>
        /// Строка информирования успешного завершения сервиса
        /// </summary>
        public override string SuccessfullEndInfo
        {
            get
            {
                return "Распаковка файла успешно выполнена. Файл: " + outputFileName;
            }
        }

        /// <summary>
        /// Получить блок входного файла
        /// </summary>
        /// <param name="inputFile">Входной файл</param>
        /// <param name="currentPos">Текущая позиция в файле</param>
        /// <param name="bytesReadSize">Длина массива результирующего блока</param>
        /// <returns>Блок входного файла</returns>
        protected override byte[] GetFileBlock(string sourceFile, long pos, int bytesRead)
        {
            byte[] res = new byte[bytesRead];
            using (FileStream compressedFile = new FileStream(sourceFile, FileMode.Open))
            {
                compressedFile.Position = pos;
                byte[] byteBuffer = new byte[8];
                compressedFile.Read(byteBuffer, 0, byteBuffer.Length);
                byte[] compressedBuffer = new byte[bytesRead];
                byteBuffer.CopyTo(compressedBuffer, 0);
                compressedFile.Read(compressedBuffer, 8, bytesRead - 8);
                res = compressedBuffer;
            }

            return res;
        }
    }
}