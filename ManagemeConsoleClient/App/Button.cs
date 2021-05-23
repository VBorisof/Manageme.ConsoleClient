using System;
using System.Linq;

namespace ManagemeConsoleClient.App
{
    public class Button
    {
        private string _text;
        private int _width;

        private int _left;
        private int _top;

        public bool IsSelected { get; set; }

        public EventHandler Pressed = new EventHandler((_, __) => {});

        public Button(string text, int left, int top, int width)
        {
            _text = text;
            _left = left;
            _top = top;
            _width = width;
        }


        public void Press()
        {
            Pressed(this, null);
        }

        public void Render()
        {
            var originalLeft = Console.CursorLeft;
            var originalTop = Console.CursorTop;
            Console.SetCursorPosition(_left, _top);

            var originalBg = Console.BackgroundColor;
            if (IsSelected)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
            }
            Console.Write($"┌{(new string('─', _width))}┐");
            
            Console.SetCursorPosition(_left, _top+1);
            var renderedText = 
                _text
                    .PadLeft((_width + _text.Count())/2)
                    .PadRight(_width);
            Console.Write($"│{renderedText}│");
            
            Console.SetCursorPosition(_left, _top+2);
            Console.Write($"└{(new string('─', _width))}┘");

            if (IsSelected)
            {
                Console.BackgroundColor = originalBg;
            }

            Console.SetCursorPosition(originalLeft, originalTop);
        }
    }
}
