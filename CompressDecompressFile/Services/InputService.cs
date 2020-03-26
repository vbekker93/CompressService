using System;
using System.IO;

namespace CompressDecompressFile.Services
{
    /// <summary>
    /// Структура входных параметров
    /// </summary>
    public struct InitArgs
    {
        /// <summary>
        /// Действие сжатия/распаковки
        /// </summary>
        public string Action { set; get; }

        /// <summary>
        /// Входной файл
        /// </summary>
        public FileInfo InputFileFullName { set; get; }

        /// <summary>
        /// Результирующий файл
        /// </summary>
        public FileInfo OutputFileFullName { set; get; }
    }

    /// <summary>
    /// Сервис инициализации входных параметров
    /// </summary>
    public static class InputService
    {
        /// <summary>
        /// Получение входных параметров из массива строк
        /// </summary>
        /// <param name="args">Входной массив</param>
        /// <param name="errMessage">Ошибки проверок параметров</param>
        /// <returns>Структура входных параметров</returns>
        public static InitArgs ParseConsoleArgs(string[] args, out string errMessage)
        {
            InitArgs result = new InitArgs();
            errMessage = string.Empty;

            if (args.Length != 3)
            {
                errMessage = "Входная строка имеет неверный формат.";
            }
            else
            {
                result.Action = args[0].ToLower();
                result.InputFileFullName = new FileInfo(args[1]);
                result.OutputFileFullName = new FileInfo(args[2]);

                if (result.Action != Constants.Compress && result.Action != Constants.Decompress)
                {
                    errMessage = "Некорректная сигнатура действия распаковки/сжатия.";
                }
                else if (!result.InputFileFullName.Exists)
                {
                    errMessage = "Входной файл " + result.InputFileFullName.FullName + " не существует! ";
                }
                else if (result.InputFileFullName == result.OutputFileFullName)
                {
                    errMessage = "Входной и выходной файл должны иметь разные пути!";
                }
                else if (result.InputFileFullName.Length == 0)
                {
                    errMessage = "Не указан выходной файл.";
                }
                else
                {
                    if (result.InputFileFullName.Extension != Constants.CompressedFileExtantion && result.Action == Constants.Decompress)
                    {
                        errMessage = "Выходной файл должен иметь расширение" + Constants.CompressedFileExtantion + "!";
                    }
                }
            }

            if (!string.IsNullOrEmpty(errMessage))
            {
                errMessage += Environment.NewLine + "Корректный формат запуска программы: CompressDecompressFile.exe compress(или decompress) [полный путь к входному файлу] [полный путь к выходному файлу]";
                return default;
            }

            return result;
        }
    }
}