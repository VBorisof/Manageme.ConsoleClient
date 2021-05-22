using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ManagemeConsoleClient.Client;
using ManagemeConsoleClient.Forms;
using ManagemeConsoleClient.ViewModels;

namespace ManagemeConsoleClient.App
{
    public class App
    {
        public bool IsRunning { get; set; }

        private int _selectedTodoIndex = 0;
        private List<TodoViewModel> _categoryTodos;

        private CategoryViewModel _currentCategory = null;
        private List<CategoryViewModel> _categories;

        private ManagemeHttpClient _client;
        
        public async Task InitAsync()
        {
            _client = new ManagemeHttpClient();

            await _client.LoginAsync(new LoginForm("Seva", "123"));
            _categories = await _client.GetCategoriesAsync();

            _currentCategory = _categories.First();
            _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);
        }

        public async Task RunAsync()
        {
            IsRunning = true;
            Console.Clear();
            Console.CursorVisible = false;

            while (IsRunning)
            {
                _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);
                if (_selectedTodoIndex < 0 
                    || _selectedTodoIndex >= _categoryTodos.Count)
                {
                    _selectedTodoIndex = 0;
                }

                Render();

                var key = Console.ReadKey(intercept: true);

                await ProcessKeyAsync(key.Key);
            }
            
            Console.Clear();
            Console.CursorVisible = true;
        }

        private void Render()
        {
            Console.SetCursorPosition(0, 0);

            Console.WriteLine("".PadRight(Console.WindowWidth, '='));

            Console.WriteLine($"{_currentCategory.Name}:".PadRight(Console.WindowWidth));
           
            var linesLeft = Console.WindowHeight - Console.CursorTop - 1;
           
            // Handle long lists
            var startIndex = Math.Max(0, _selectedTodoIndex - linesLeft);
            var endIndex = Math.Min(_categoryTodos.Count - 1, startIndex + linesLeft);

            var originalBg = Console.BackgroundColor;
            for (int i = startIndex; i <= endIndex; ++i)
            {
                if (i == _selectedTodoIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }

                Console.Write((_categoryTodos[i].IsDone ? "☑ " : "☐ " ).PadLeft(2));
                Console.WriteLine(_categoryTodos[i].Content.PadRight(Console.WindowWidth-2));

                if (i == _selectedTodoIndex)
                {
                    Console.BackgroundColor = originalBg;
                }
            }

            // -2 Comes from last two lines (sep + status)
            linesLeft = Console.WindowHeight - Console.CursorTop - 2;

            for (int i = 0; i < linesLeft; ++i)
            {
                Console.WriteLine("".PadRight(Console.WindowWidth));
            }

            Console.WriteLine("".PadRight(Console.WindowWidth, '='));

            Console.Write("Hit h for help.".PadRight(Console.WindowWidth));
        }

        private void ClearCurrentLine()
        {
            string spaces = new string(' ', Console.WindowWidth);
            Console.Write($"\r{spaces}\r");
        }

        private void SelectNextTodo()
        {
            if (++_selectedTodoIndex >= _categoryTodos.Count)
            {
                _selectedTodoIndex = 0;
            }
        }

        private void SelectPrevTodo()
        {
            if (--_selectedTodoIndex < 0)
            {
                _selectedTodoIndex = _categoryTodos.Count - 1;
            }
        }

        private async Task ProcessKeyAsync(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.J:
                case ConsoleKey.DownArrow:
                    SelectNextTodo();
                    break;

                case ConsoleKey.K:
                case ConsoleKey.UpArrow:
                    SelectPrevTodo();
                    break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    await _client.ToggleTodoDoneAsync(
                        _categoryTodos[_selectedTodoIndex].Id
                    );
                    break;

                case ConsoleKey.D:
                    if (! _categoryTodos.Any())
                    {
                        break;
                    }
                    ClearCurrentLine();
                    Console.Write("Delete current TODO. Are you sure? (yN)");

                    var decision = Console.ReadKey(intercept: false).Key;
                    if (decision == ConsoleKey.Y)
                    {
                        await _client.DeleteTodoAsync(
                            _categoryTodos[_selectedTodoIndex].Id
                        );
                    }
                    break;
                
                case ConsoleKey.A:
                    ClearCurrentLine();
                    Console.Write("Add TODO (Leave blank to cancel): ");

                    var content = Console.ReadLine();
                    if (! string.IsNullOrWhiteSpace(content))
                    {
                        await _client.AddTodoAsync(
                            new TodoForm
                            {
                                CategoryId = _currentCategory.Id,
                                Content = content
                            }
                        );
                    }
                    break;

                case ConsoleKey.H:
                    Console.Clear();
                    Console.WriteLine(File.ReadAllText("res/help.txt"));
                    Console.ReadKey(intercept: true);
                    break;

                case ConsoleKey.X:
                    IsRunning = false;
                    break;
            }
        }
    }
}
