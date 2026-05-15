using TradingJournal.Application.Helper;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Queries;

namespace TradingJournal.Application.Handlers.Trades.Queries
{
    public class GetTradeByIdQueryHandler : IQueryHandler<GetTradeByIdQuery, BaseResponse<TradeDto>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public GetTradeByIdQueryHandler(ITradeRepository tradeRepository, IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<TradeDto>> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<TradeDto>.Unauthorized("User not found.");

            var trade = await _tradeRepository.GetOneAsyncUntracked<Trade>(
                filter: t => t.Id == request.Id && t.UserId == user.Id);

            if (trade is null)
                return BaseResponse<TradeDto>.NotFound($"Trade with ID '{request.Id}' was not found.");

            return BaseResponse<TradeDto>.Ok(trade.ToDto());
        }
    }
}
