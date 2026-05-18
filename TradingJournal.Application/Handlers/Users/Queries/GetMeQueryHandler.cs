using TradingJournal.Application.Interfaces;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Users.Queries;

namespace TradingJournal.Application.Handlers.Users.Queries
{
    public class GetMeQueryHandler : IQueryHandler<GetMeQuery, BaseResponse<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAdminSettings _adminSettings;

        public GetMeQueryHandler(IUserRepository userRepository, IAdminSettings adminSettings)
        {
            _userRepository = userRepository;
            _adminSettings = adminSettings;
        }

        public async Task<BaseResponse<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsyncUntracked<User>(
                filter: u => u.Auth0Id == request.Auth0Id);

            if (user is null)
                return BaseResponse<UserDto>.NotFound("User not found.");

            return BaseResponse<UserDto>.Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                DailyLossLimit = user.DailyLossLimit,
                DailyProfitTarget = user.DailyProfitTarget,
                IsAdmin = _adminSettings.IsAdmin(user.Email)
            });
        }
    }
}
