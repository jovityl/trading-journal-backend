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

        public GetMeQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsyncUntracked<UserDto>(
                filter: u => u.Auth0Id == request.Auth0Id,
                selector: u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    DailyLossLimit = u.DailyLossLimit,
                    DailyProfitTarget = u.DailyProfitTarget
                });

            if (user is null)
                return BaseResponse<UserDto>.NotFound("User not found.");

            return BaseResponse<UserDto>.Ok(user);
        }
    }
}
