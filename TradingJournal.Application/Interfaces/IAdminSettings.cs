namespace TradingJournal.Application.Interfaces
{
    /// <summary>
    /// Tells whether a given email is an admin.
    /// Implementation lives in Infrastructure (reads from config).
    /// Application code depends only on this abstraction — no IConfiguration.
    /// </summary>
    public interface IAdminSettings
    {
        bool IsAdmin(string email);
    }
}
