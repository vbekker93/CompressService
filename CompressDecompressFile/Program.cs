using CompressDecompressFile.Services;
using CompressDecompressFile.Utils;
using System;

namespace CompressDecompressFile
{
    public static class Program
    {
        /// <summary>
        /// Объект анимации процесса обработки
        /// </summary>
        private static ConsoleSpinner loadSpinner;

        public static void Main(string[] args)
        {
            try
            {
                InitArgs initParams = InputService.ParseConsoleArgs(args, out string errors);

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine(errors);
                }
                else
                {
                    Console.WriteLine("Входные параметры успешно загружены. ");
                    Console.WriteLine("Подготовка к обработке файла " + initParams.InputFileFullName.FullName);

                    switch (initParams.Action)
                    {
                        case Constants.Compress:
                            ProcessStart(new CompressService(initParams.InputFileFullName.FullName, initParams.OutputFileFullName.FullName));
                            break;

                        case Constants.Decompress:
                            ProcessStart(new DecompressService(initParams.InputFileFullName.FullName, initParams.OutputFileFullName.FullName));
                            break;
                    }
                    GC.Collect();
                }

                Console.WriteLine("Нажмите любую кнопку для выхода.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка во время выполнения: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Запуск сервиса обработки файла
        /// </summary>
        /// <param name="service">Объект сервиса обработки</param>
        public static void ProcessStart(ICompressActionsService service)
        {
            try
            {
                Console.Write(service.StartInfoConst);

                loadSpinner = new ConsoleSpinner(Console.CursorLeft, Console.CursorTop);
                loadSpinner.Start();
                service.Start(out string errors);
                loadSpinner.Stop();

                if (!string.IsNullOrEmpty(errors))
                    Console.WriteLine(errors);
                else
                    Console.WriteLine(service.SuccessfullEndInfo);
            }
            catch (Exception ex)
            {
                loadSpinner.Stop();
                throw ex;
            }
        }
    }
}