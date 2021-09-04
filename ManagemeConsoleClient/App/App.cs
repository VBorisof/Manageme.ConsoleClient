using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
       
        private ReminderPopup _popup;

        public async Task InitAsync(LoginForm form)
        {
            _client = new ManagemeHttpClient();
            await _client.LoginAsync(form);

            _categories = await _client.GetCategoriesAsync();

            _currentCategory = _categories.FirstOrDefault();
    
            if (_currentCategory == null)
            {
                _currentCategory = await _client.AddCategoryAsync(new CategoryForm("General"));
            }

            _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);
                
            _popup = new ReminderPopup(_client);
        }

        public async Task RunAsync()
        {
            _state = AppState.Running;

            Console.Clear();
            Console.CursorVisible = false;

            var updateThread = 
                new Thread(async () =>
                {
                    while (_state != AppState.Stopped)
                    {
                        if (_state != AppState.Input)
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
                        if (_state != AppState.Input)
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

                var key = Console.ReadKey(intercept: true);

                await ProcessKeyAsync(key.Key);
            }
           
            updateThread.Join();
            renderThread.Join();

            Console.Clear();
            Console.CursorVisible = true;
        }

        private void Render()
        {
            Renderer.Clear();


            // Print the bottom first -- works better.
            Renderer.RenderBottomSeparator();

            Console.Write("Hit h for help.");

            // Print the header
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{Renderer.GetSeparator()}\n{_currentCategory.Name}:");
           
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

            Renderer.SetCursorBottomMenu();
            
            if (_state == AppState.Popup)
            {
                _popup.Render();
            }
        }

        private async Task UpdateAsync()
        {
            _categoryTodos = await _client.GetTodosAsync(_currentCategory.Id);
            if (_selectedTodoIndex < 0
                || _selectedTodoIndex >= _categoryTodos.Count)
            {
                _selectedTodoIndex = 0;
            }

            var reminders = await _client.GetRemindersAsync();
            if (reminders.Count > 0)
            {
                _popup.SetWindow(5, 2, Console.WindowWidth - 10, 20);
                _popup.IsOpen = true;
                _popup.Reminders = reminders;
                _state = AppState.Popup;
            }
            else
            {
                _popup.IsOpen = false;
                _state = AppState.Running;
            }
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
                    {
                        if (! _categoryTodos.Any())
                        {
                            break;
                        }
                        await _client.ToggleTodoDoneAsync(
                            _categoryTodos[_selectedTodoIndex].Id
                        );
                        break;
                    }

                case ConsoleKey.D:
                    {
                        if (! _categoryTodos.Any())
                        {
                            break;
                        }

                        _state = AppState.Input;

                        Renderer.ClearCurrentLine();
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
                    }

                case ConsoleKey.A:
                    {
                        Renderer.ClearCurrentLine();
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
                    }

                case ConsoleKey.R:
                    {
                        Renderer.ClearCurrentLine();
                        Console.Write("Add Reminder (Leave blank to cancel): ");

                        _state = AppState.Input;
                        var content = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            _state = AppState.Running;
                            return;
                        }

                        Renderer.ClearCurrentLine();
                        Console.Write(
                            "Pick time (Any other char to cancel):\n"
                            + "1 - Today EOD\n"
                            + "2 - Tomorrow Morning\n"
                            + "3 - Tomorrow EOD\n"
                            + "4 - [DEBUG] In 5 seconds\n"
                            + "0 - Custom Time"
                        );

                        var timeChoice = Console.ReadKey(intercept: true).Key;
                        DateTime time;

                        switch(timeChoice)
                        {
                            case ConsoleKey.D1:
                                time = DateTime.Today 
                                    + TimeSpan.FromHours(17);
                                break;
                            case ConsoleKey.D2:
                                time = DateTime.Today
                                    + TimeSpan.FromDays(1) 
                                    + TimeSpan.FromHours(8);
                                break;
                            case ConsoleKey.D3:
                                time = DateTime.Today 
                                    + TimeSpan.FromDays(1)
                                    + TimeSpan.FromHours(17);
                                break;
                            case ConsoleKey.D4:
                                time = DateTime.Now + TimeSpan.FromSeconds(5);
                                break;
 
                            //    
                            //case ConsoleKey.D0:
                            //    break;
                            
                            default:
                                _state = AppState.Running;
                                return;
                        }

                        time = time.ToUniversalTime();

                        await _client.AddReminderAsync(
                            new ReminderForm
                            {
                                CategoryId = _currentCategory.Id,
                                Content = content,
                                Time = time
                            }
                        );
                        _state = AppState.Running;
                        break;
                    }

                case ConsoleKey.H:
                    {
                        _state = AppState.Input;

                        Console.Clear();
                        Console.WriteLine(File.ReadAllText("res/help.txt"));
                        Console.ReadKey(intercept: true);

                        _state = AppState.Running;
                        break;
                    }
                    
                case ConsoleKey.X:
                    _state = AppState.Stopped;
                    break;
            }
        }
    }
}

