namespace TradingJournal.Contract.DTOs
{
    public class PromptDto
    {
        public string Key { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
