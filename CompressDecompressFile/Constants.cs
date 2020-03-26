namespace CompressDecompressFile
{
    /// <summary>
    /// Константы программы
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Расширение сжатого файла
        /// </summary>
        public const string CompressedFileExtantion = ".gz";

        /// <summary>
        /// Действие сжатия файла
        /// </summary>
        public const string Compress = "compress";

        /// <summary>
        /// Действие распаковки файла
        /// </summary>
        public const string Decompress = "decompress";

        /// <summary>
        /// Размер блока обработки файла
        /// </summary>
        public const int SizeBlock = 10000000;

        /// <summary>
        /// Время ожидания потоков
        /// </summary>
        public const int ThreadDelay = 100;

        /// <summary>
        /// Максимальное кол-во обрабатываемых блоков
        /// </summary>
        public const int maxProcessedBlocks = 10;
    }
}