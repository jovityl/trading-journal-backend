using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Users.Commands;

namespace TradingJournal.Application.Handlers.Users.Commands
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, BaseResponse<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (existingUser is not null)
                return BaseResponse<UserDto>.Ok(new UserDto
                {
                    Id = existingUser.Id,
                    Email = existingUser.Email,
                    DisplayName = existingUser.DisplayName,
                    DailyLossLimit = existingUser.DailyLossLimit,
                    DailyProfitTarget = existingUser.DailyProfitTarget
                });

            var user = new User
            {
                Id = Guid.NewGuid(),
                Auth0Id = request.Auth0Id,
                Email = request.Email,
                DisplayName = request.DisplayName,
                DailyLossLimit = 0,
                DailyProfitTarget = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            return BaseResponse<UserDto>.Created(new UserDto
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
