using System;

namespace ManagemeConsoleClient.ViewModels
{
    public class ReminderViewModel
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public long CategoryId { get; set; }
        public DateTime? Time { get; set; }
    }
}
