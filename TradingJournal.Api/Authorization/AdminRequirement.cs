using Microsoft.AspNetCore.Authorization;
using TradingJournal.Application.Interfaces;
using TradingJournal.Domain.IRepository;

namespace TradingJournal.Api.Authorization
{
    public class AdminRequirement : IAuthorizationRequirement { }

    public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly IAdminSettings _adminSettings;
        private readonly IUserRepository _userRepository;

        public AdminRequirementHandler(IAdminSettings adminSettings, IUserRepository userRepository)
        {
            _adminSettings = adminSettings;
            _userRepository = userRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminRequirement requirement)
        {
            var auth0Id = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0Id))
                return;

            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == auth0Id);
            if (user is null)
                return;

            if (_adminSettings.IsAdmin(user.Email))
            {
                context.Succeed(requirement);
            }
        }
    }
}
