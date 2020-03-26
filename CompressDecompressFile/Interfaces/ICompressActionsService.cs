namespace CompressDecompressFile.Services
{
    /// <summary>
    /// Контракт поведения сервисов сжатия/распаковки
    /// </summary>
    public interface ICompressActionsService
    {
        /// <summary>
        /// Строка информирования начала работы сервиса
        /// </summary>
        string StartInfoConst { get; }

        /// <summary>
        /// Строка информирования успешного завершения сервиса
        /// </summary>
        string SuccessfullEndInfo { get; }

        /// <summary>
        /// Запустить обработку сервиса
        /// </summary>
        void Start(out string errors);
    }
}