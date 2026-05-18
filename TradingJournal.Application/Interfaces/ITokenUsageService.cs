namespace TradingJournal.Application.Interfaces
{
    public interface ITokenUsageService
    {
        Task RecordAsync(Guid userId, string endpoint, AiUsage usage, CancellationToken cancellationToken = default);
    }
}
