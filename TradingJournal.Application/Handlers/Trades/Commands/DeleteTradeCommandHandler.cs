using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class DeleteTradeCommandHandler : ICommandHandler<DeleteTradeCommand, BaseResponse<bool>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public DeleteTradeCommandHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteTradeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<bool>.Unauthorized("User not found.");

            var trade = await _tradeRepository.GetByIdAsync(request.Id);
            if (trade is null)
                return BaseResponse<bool>.NotFound($"Trade with ID '{request.Id}' was not found.");

            if (trade.UserId != user.Id)
                return BaseResponse<bool>.Unauthorized("You are not authorized to delete this trade.");

            await _tradeRepository.DeleteAsync(trade);

            return BaseResponse<bool>.Ok(true, "Trade deleted successfully.");
        }
    }
}
