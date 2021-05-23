using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagemeConsoleClient.Client;
using ManagemeConsoleClient.ViewModels;

namespace ManagemeConsoleClient.App
{
    public class ReminderPopup : Window
    {
        private List<ReminderViewModel> _reminders;
        private int _selectedReminderIndex;

        public ReminderPopup(
            ManagemeHttpClient client,
            List<ReminderViewModel> reminders,
            int left,
            int top,
            int width,
            int height
        ) : base(
                client,
                " R E M I N D E R ",
                left, top, width, height
            )
        {
            _reminders = reminders;
            _selectedReminderIndex = 0;

            var okButton = new Button("OK", _left + 15, _top+height - 3, 15);
            okButton.Pressed += (_, __) =>
            {
                IsOpen = false;
            };

            Buttons = new List<Button>
            {
                //okButton
            };
        }

        private void SelectNextReminder()
        {
            if (++_selectedReminderIndex >= _reminders.Count)
            {
                _selectedReminderIndex = 0;
            }
        }

        private void SelectPrevReminder()
        {
            if (--_selectedReminderIndex < 0)
            {
                _selectedReminderIndex = _reminders.Count - 1;
            }
        }
        
        public override void Render()
        {
            base.Render();

            var originalBg = Console.BackgroundColor;
            for (int i = 0; i < _reminders.Count; ++i)
            {
                if (i == _selectedReminderIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }

                Console.SetCursorPosition(_left + 4, _top+2 + i);
                Console.Write(_reminders[i].Content);

                if (i == _selectedReminderIndex)
                {
                    Console.BackgroundColor = originalBg;
                }
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
                    _reminders.RemoveAt(_selectedReminderIndex);
                    if (_selectedReminderIndex >= _reminders.Count)
                    {
                        SelectNextReminder();
                    }
                    break;
                    
                case ConsoleKey.S:
                    // Snooze reminder for some time...
                    // And remove it...
                    _reminders.RemoveAt(_selectedReminderIndex);
                    if (_selectedReminderIndex >= _reminders.Count)
                    {
                        SelectNextReminder();
                    }
                    break;
            }

            // TODO: Either move this to some new Update(),
            // Or rename current func to Update, kind of smelly here...
            if (! _reminders.Any())
            {
                IsOpen = false;
            }
        }
    }
}
