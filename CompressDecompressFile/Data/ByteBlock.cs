namespace CompressDecompressFile.Data
{
    /// <summary>
    /// Сущность минимальной единицы данных сжатия/распаковки
    /// </summary>
    public class ByteBlock
    {
        public ByteBlock()
        {
        }

        public ByteBlock(int id, byte[] data)
        {
            ID = id;
            this.Buffer = data;
        }

        /// <summary>
        /// Идентификатор блока
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Массив данных
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Строковое представление объекта
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"ID = {ID}; length = {Buffer.Length}";
    }
}