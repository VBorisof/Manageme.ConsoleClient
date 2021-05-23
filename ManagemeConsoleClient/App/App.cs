using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagemeConsoleClient.Client;
using ManagemeConsoleClient.Forms;
using ManagemeConsoleClient.ViewModels;

namespace ManagemeConsoleClient.App
{
    public class App
    {
        private AppState _state { get; set; }

        private int _selectedTodoIndex = 0;
        private List<TodoViewModel> _categoryTodos;

        private CategoryViewModel _currentCategory = null;
        private List<CategoryViewModel> _categories;

        private ManagemeHttpClient _client;
       
        private Window _popup;

        public async Task InitAsync()
        {
            _client = new ManagemeHttpClient();

            await _client.LoginAsync(new LoginForm("Seva", "123"));
            _categories = await _client.GetCategoriesAsync();

            _currentCategory = _categories.First();
            _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);

            await OpenPopup();
        }

        public async Task OpenPopup()
        {
            var reminders = await _client.GetRemindersAsync();

            _popup = new ReminderPopup(
                _client,
                reminders,
                5, 2, Console.WindowWidth - 10, 20
            );
            _popup.IsOpen = true;
            _state = AppState.Popup;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            Console.CursorVisible = false;

            var updateThread = 
                new Thread(async () =>
                {
                    while (_state != AppState.Stopped)
                    {
                        if (_state == AppState.Running)
                        {
                            await UpdateAsync();
                        }
                        Thread.Sleep(5000);
                    }
                }
            );
            updateThread.Start();

            var renderThread = 
                new Thread(() =>
                {
                    while (_state != AppState.Stopped)
                    {
                        if (_state == AppState.Running)
                        {
                            Render();
                        }
                        Thread.Sleep(5000);
                    }
                }
            );
            renderThread.Start();

            while (_state != AppState.Stopped)
            {
                await UpdateAsync();
                Render();

                if (_state == AppState.Popup)
                {
                    _popup.Render();
                }

                var key = Console.ReadKey(intercept: true);

                await ProcessKeyAsync(key.Key);
            }
           
            updateThread.Join();
            renderThread.Join();

            Console.Clear();
            Console.CursorVisible = true;
        }

        private void Clear()
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

        private void Render()
        {
            Clear();

            var separator = "".PadRight(Console.WindowWidth, '=');

            // Print the bottom first -- works better.
            Console.SetCursorPosition(0, Console.WindowHeight - 2);

            Console.WriteLine(separator);
            Console.Write("Hit h for help.");

            // Print the header
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{separator}\n{_currentCategory.Name}:");
           
            var linesLeft = Console.WindowHeight - Console.CursorTop - 2;
           
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
                Console.WriteLine(_categoryTodos[i].Content);

                if (i == _selectedTodoIndex)
                {
                    Console.BackgroundColor = originalBg;
                }
            }

            Console.SetCursorPosition(0, Console.WindowHeight-1);
        }

        private async Task UpdateAsync()
        {
            _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);
            if (_selectedTodoIndex < 0
                || _selectedTodoIndex >= _categoryTodos.Count)
            {
                _selectedTodoIndex = 0;
            }
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
            if (key == ConsoleKey.Escape)
            {
                _state = AppState.Running;
                return;
            }

            if (_state == AppState.Popup)
            {
                await _popup.ProcessKeyAsync(key);
                if (! _popup.IsOpen)
                {
                    _state = AppState.Running;
                }
                return;
            }

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
                    if (! _categoryTodos.Any())
                    {
                        break;
                    }
                    await _client.ToggleTodoDoneAsync(
                        _categoryTodos[_selectedTodoIndex].Id
                    );
                    break;

                case ConsoleKey.D:
                    if (! _categoryTodos.Any())
                    {
                        break;
                    }

                    _state = AppState.Input;

                    ClearCurrentLine();
                    Console.Write("Delete current TODO. Are you sure? (yN)");

                    var decision = Console.ReadKey(intercept: false).Key;
                    if (decision == ConsoleKey.Y)
                    {
                        await _client.DeleteTodoAsync(
                            _categoryTodos[_selectedTodoIndex].Id
                        );
                    }

                    _state = AppState.Running;
                    break;
                
                case ConsoleKey.A:
                    ClearCurrentLine();
                    Console.Write("Add TODO (Leave blank to cancel): ");

                    _state = AppState.Input;
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
                    _state = AppState.Running;
                    break;

                case ConsoleKey.H:
                    _state = AppState.Input;

                    Console.Clear();
                    Console.WriteLine(File.ReadAllText("res/help.txt"));
                    Console.ReadKey(intercept: true);

                    _state = AppState.Running;
                    break;

                case ConsoleKey.X:
                    _state = AppState.Stopped;
                    break;
            }
        }
    }
}
