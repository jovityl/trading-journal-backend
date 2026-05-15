using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Users.Commands;

namespace TradingJournal.Application.Handlers.Users.Commands
{
    public class UpdateUserLimitsCommandHandler : ICommandHandler<UpdateUserLimitsCommand, BaseResponse<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserLimitsCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<UserDto>> Handle(UpdateUserLimitsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<UserDto>.NotFound("User not found.");

            user.DailyLossLimit = request.DailyLossLimit;
            user.DailyProfitTarget = request.DailyProfitTarget;

            await _userRepository.UpdateAsync(user);

            return BaseResponse<UserDto>.Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                DailyLossLimit = user.DailyLossLimit,
                DailyProfitTarget = user.DailyProfitTarget
            });
        }
    }
}
