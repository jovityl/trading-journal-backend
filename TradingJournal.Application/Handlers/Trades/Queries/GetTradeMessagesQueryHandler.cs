using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Queries;

namespace TradingJournal.Application.Handlers.Trades.Queries
{
    public class GetTradeMessagesQueryHandler : IQueryHandler<GetTradeMessagesQuery, BaseResponse<List<TradeMessageDto>>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITradeMessageRepository _messageRepository;

        public GetTradeMessagesQueryHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            ITradeMessageRepository messageRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        public async Task<BaseResponse<List<TradeMessageDto>>> Handle(GetTradeMessagesQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<List<TradeMessageDto>>.Unauthorized("User not found.");

            // Make sure the trade belongs to the user
            var trade = await _tradeRepository.GetOneAsyncUntracked<Trade>(
                filter: t => t.Id == request.TradeId && t.UserId == user.Id);
            if (trade is null)
                return BaseResponse<List<TradeMessageDto>>.NotFound("Trade not found.");

            var messages = await _messageRepository.GetListAsyncUntracked<TradeMessage>(
                filter: m => m.TradeId == request.TradeId);

            var result = messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new TradeMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    Model = m.Model,
                    CreatedAt = m.CreatedAt
                })
                .ToList();

            return BaseResponse<List<TradeMessageDto>>.Ok(result);
        }
    }
}
