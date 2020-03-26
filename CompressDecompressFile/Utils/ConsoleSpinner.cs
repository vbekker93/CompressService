using System;
using System.Threading;

namespace CompressDecompressFile.Utils
{
    /// <summary>
    /// Анимация процесса обработки
    /// </summary>
    public class ConsoleSpinner : IDisposable
    {
        /// <summary>
        /// Символ очистки
        /// </summary>
        private const char Sequence = ' ';

        /// <summary>
        /// Позиция анимации
        /// </summary>
        private int leftPosition = 0;

        /// <summary>
        /// Количество анимационных символов
        /// </summary>
        private int barLength = 5;

        /// <summary>
        /// Координата Х в консоли
        /// </summary>
        private readonly int left;

        /// <summary>
        /// Координата У в консоли
        /// </summary>
        private readonly int top;

        /// <summary>
        /// Время ожидания
        /// </summary>
        private readonly int delay;

        /// <summary>
        /// Признак активности анимации
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Поток исполнения анимации
        /// </summary>
        private readonly Thread thread;

        public ConsoleSpinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        /// <summary>
        /// Старт анимации
        /// </summary>
        public void Start()
        {
            Console.CursorVisible = false;

            isActive = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        /// <summary>
        /// Завершение анимации
        /// </summary>
        public void Stop()
        {
            isActive = false;

            for (int i = 0; left + i < left + barLength + 1; i++)
            {
                Console.SetCursorPosition(left + i, top);
                Console.Write(' ');
            }
            Console.WriteLine();

            Console.CursorVisible = true;
        }

        /// <summary>
        /// Активация потока
        /// </summary>
        private void Spin()
        {
            while (isActive)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Отрисовка анимационных символов
        /// </summary>
        /// <param name="c"></param>
        private void Draw(char c)
        {
            Console.SetCursorPosition(left + leftPosition - 1, top);
            Console.Write('.');

            Console.SetCursorPosition(left + leftPosition, top);
            Console.Write(c);

            if (leftPosition > barLength)
                leftPosition = 1;
            else
                leftPosition++;
        }

        /// <summary>
        /// Включение отрисовки
        /// </summary>
        private void Turn()
        {
            Draw(Sequence);
        }

        /// <summary>
        /// Завершение анимации
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}