using System;
using System.Text;

namespace ManagemeConsoleClient.App
{
    public static class Renderer
    {
        public static void Clear()
        {
            Console.SetCursorPosition(0, 0);

            var sb = new StringBuilder();
            var clearStr = "".PadRight(Console.WindowWidth);
            for (int i = 0; i <= Console.WindowHeight; ++i)
            {
                sb.AppendLine(clearStr);
            }

            Console.Write(sb);

            Console.SetCursorPosition(0, 0);
        }

        public static void ClearCurrentLine()
        {
            string spaces = new string(' ', Console.WindowWidth);
            Console.Write($"\r{spaces}\r");
        }

        public static void SetCursorBottomMenu()
        {
            Console.SetCursorPosition(0, Console.WindowHeight-1);
        }

        public static string GetSeparator()
        {
            return "".PadRight(Console.WindowWidth, '=');
        }

        public static void RenderBottomSeparator()
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 2);

            Console.WriteLine(GetSeparator());
        }
    }
}

