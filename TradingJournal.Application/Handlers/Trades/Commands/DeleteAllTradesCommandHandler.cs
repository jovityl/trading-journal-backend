using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class DeleteAllTradesCommandHandler : ICommandHandler<DeleteAllTradesCommand, BaseResponse<int>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public DeleteAllTradesCommandHandler(ITradeRepository tradeRepository, IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<int>> Handle(DeleteAllTradesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<int>.Unauthorized("User not found.");

            var trades = await _tradeRepository.GetListAsync(filter: t => t.UserId == user.Id);
            var count = trades.Count();

            foreach (var trade in trades)
                await _tradeRepository.DeleteAsync(trade);

            return BaseResponse<int>.Ok(count, $"Deleted {count} trades.");
        }
    }
}
