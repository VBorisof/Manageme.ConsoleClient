namespace ManagemeConsoleClient.ViewModels
{
    public class TodoViewModel
    {
        public long Id { get; set; }
        public bool IsDone { get; set; }
        public string Content { get; set; }
        public long CategoryId { get; set; }
    }
}
