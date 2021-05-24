using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagemeConsoleClient.App
{
    public abstract class Window
    {
        public bool IsOpen { get; set; }

        protected int _left;
        protected int _top;
        protected int _width;
        protected int _height;
        private string _title;

        protected List<Button> Buttons { get; set; }
        private int _selectedButtonIndex;

        public Window(string title)
        {
            _title = title;
            _selectedButtonIndex = 0;
            Buttons = new List<Button>();
        }


        public void SetWindow(int left, int top, int width, int height)
        {
            _left = left;
            _top = top;
            _width = width;
            _height = height;
        }
        
        public virtual void Render()
        {
            var originalLeft = Console.CursorLeft;
            var originalTop = Console.CursorTop;

            _left = 5;
            _top = 2;
            _width = Console.WindowWidth - 10;
            _height = 20;

            Console.SetCursorPosition(_left, _top);

            Console.Write($"╔{(new string('═', _width))}╗");
            for (int i = 1; i <= _height; ++i)
            {
                Console.SetCursorPosition(_left, _top + i);
                Console.WriteLine($"║{(" ".PadRight(_width))}║");
            }

            Console.SetCursorPosition(_left, _top + _height);
            Console.Write($"╚{(new string('═', _width))}╝");

            Console.SetCursorPosition(
                _left + _width/2 - _title.Count()/2,
                _top
            );
            Console.Write(_title);

            Buttons.ForEach(b => b.Render());

            Console.SetCursorPosition(originalLeft, originalTop);
        }

        public async virtual Task ProcessKeyAsync(ConsoleKey key)
        {
            switch(key)
            {
                case ConsoleKey.H:
                case ConsoleKey.LeftArrow:
                    if (! Buttons.Any())
                    {
                        break;
                    }
                    SelectPrevButton();
                    break;

                case ConsoleKey.L:
                case ConsoleKey.RightArrow:
                    if (! Buttons.Any())
                    {
                        break;
                    }
                    SelectNextButton();
                    break;
                
                case ConsoleKey.Enter:
                    if (! Buttons.Any())
                    {
                        break;
                    }
                    Buttons[_selectedButtonIndex].Press();
                    break;
            } 
        }

        private void SelectNextButton()
        {
            Buttons[_selectedButtonIndex].IsSelected = false;
            if (++_selectedButtonIndex >= Buttons.Count)
            {
                _selectedButtonIndex = 0;
            }
            Buttons[_selectedButtonIndex].IsSelected = true;
        }

        private void SelectPrevButton()
        {
            Buttons[_selectedButtonIndex].IsSelected = false;
            if (--_selectedButtonIndex < 0)
            {
                _selectedButtonIndex = Buttons.Count - 1;
            }
            Buttons[_selectedButtonIndex].IsSelected = true;
        }

    }
}
