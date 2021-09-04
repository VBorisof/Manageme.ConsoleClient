using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagemeConsoleClient.Client;
using ManagemeConsoleClient.Forms;
using ManagemeConsoleClient.ViewModels;

namespace ManagemeConsoleClient.App
{
    public class ReminderPopup : Window
    {
        private readonly ManagemeHttpClient _client;

        public List<ReminderViewModel> Reminders { get; set; }
        private int _selectedReminderIndex;

        private bool _isRenderSnoozeMenu;

        public ReminderPopup(ManagemeHttpClient client) 
            : base(" R E M I N D E R ")
        {
            _client = client;
            _selectedReminderIndex = 0;
            Reminders = new List<ReminderViewModel>();
        }

        public override void Render()
        {
            base.Render();

            var originalBg = Console.BackgroundColor;
            for (int i = 0; i < Reminders.Count; ++i)
            {
                if (i == _selectedReminderIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }

                Console.SetCursorPosition(_left + 4, _top+2 + i);
                Console.Write(Reminders[i].Content);

                if (i == _selectedReminderIndex)
                {
                    Console.BackgroundColor = originalBg;
                }
            }

            if (_isRenderSnoozeMenu)
            {
                Console.CursorTop += 2;
                Console.CursorLeft = _left + 4;
                
                Console.Write("Snooze reminder...");
                Console.CursorTop += 1;
                Console.CursorLeft = _left + 4;
                Console.Write("1 - For an hour.");
                Console.CursorTop += 1;
                Console.CursorLeft = _left + 4;
                Console.Write("2 - Tomorrow morning.");
                Console.CursorTop += 1;
                Console.CursorLeft = _left + 4;
                Console.Write("3 - Tomorrow EOD");
                Console.CursorTop += 1;
                Console.CursorLeft = _left + 4;
                Console.Write("4 - [DEBUG] For 5 seconds");
                Console.CursorTop += 1;
                Console.CursorLeft = _left + 4;
                Console.Write("0 - Custom time");
            }
        }

        public override async Task ProcessKeyAsync(ConsoleKey key)
        {
            await base.ProcessKeyAsync(key);

            switch(key)
            {
                case ConsoleKey.J:
                case ConsoleKey.DownArrow:
                    SelectNextReminder();
                    break;

                case ConsoleKey.K:
                case ConsoleKey.UpArrow:
                    SelectPrevReminder();
                    break;
                
                case ConsoleKey.A:
                    // Aknowledge and dismiss reminder.
                    // And remove it...
                    await _client.AcknowledgeReminderAsync(
                        Reminders[_selectedReminderIndex].Id
                    );
                    Reminders.RemoveAt(_selectedReminderIndex);
                    if (_selectedReminderIndex >= Reminders.Count)
                    {
                        SelectNextReminder();
                    }
                    break;
                    
                case ConsoleKey.S:
                    // Snooze reminder for some time...
                    // And remove it...
                    
                    _isRenderSnoozeMenu = true;
                    Render();

                    var timeChoice = Console.ReadKey(intercept: true).Key;
                    DateTime time;

                    switch(timeChoice)
                    {
                        case ConsoleKey.D1:
                            time = DateTime.Now 
                                + TimeSpan.FromHours(1);
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
                            _isRenderSnoozeMenu = false;
                            return;
                    }


                    time = time.ToUniversalTime();

                    await _client.SnoozeReminderAsync(
                        new SnoozeReminderForm
                        {
                            Id = Reminders[_selectedReminderIndex].Id,
                            Time = time
                        }
                    );

                    _isRenderSnoozeMenu = false;

                    Reminders.RemoveAt(_selectedReminderIndex);
                    if (_selectedReminderIndex >= Reminders.Count)
                    {
                        SelectNextReminder();
                    }
                    break;
            }

            // TODO: Either move this to some new Update(),
            // Or rename current func to Update, kind of smelly here...
            if (! Reminders.Any())
            {
                IsOpen = false;
            }
        }

        private void SelectNextReminder()
        {
            if (++_selectedReminderIndex >= Reminders.Count)
            {
                _selectedReminderIndex = 0;
            }
        }

        private void SelectPrevReminder()
        {
            if (--_selectedReminderIndex < 0)
            {
                _selectedReminderIndex = Reminders.Count - 1;
            }
        }
    }
}
