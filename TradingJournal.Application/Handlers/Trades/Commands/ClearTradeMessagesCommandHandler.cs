using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class ClearTradeMessagesCommandHandler : ICommandHandler<ClearTradeMessagesCommand, BaseResponse<int>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITradeMessageRepository _messageRepository;

        public ClearTradeMessagesCommandHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            ITradeMessageRepository messageRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        public async Task<BaseResponse<int>> Handle(ClearTradeMessagesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<int>.Unauthorized("User not found.");

            // Verify trade ownership
            var trade = await _tradeRepository.GetOneAsyncUntracked<Trade>(
                filter: t => t.Id == request.TradeId && t.UserId == user.Id);
            if (trade is null)
                return BaseResponse<int>.NotFound("Trade not found.");

            var messages = await _messageRepository.GetListAsync(filter: m => m.TradeId == request.TradeId);
            var count = messages.Count();

            foreach (var m in messages)
                await _messageRepository.DeleteAsync(m);

            return BaseResponse<int>.Ok(count, $"Deleted {count} messages.");
        }
    }
}
