using Microsoft.Extensions.Configuration;
using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class AdminSettings : IAdminSettings
    {
        private readonly HashSet<string> _adminEmails;

        public AdminSettings(IConfiguration configuration)
        {
            var emails = configuration.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();
            _adminEmails = new HashSet<string>(emails, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAdmin(string email) => !string.IsNullOrEmpty(email) && _adminEmails.Contains(email);
    }
}
