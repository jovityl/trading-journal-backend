using TradingJournal.Application.Helper;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Queries;

namespace TradingJournal.Application.Handlers.Trades.Queries
{
    public class GetTradesQueryHandler : IQueryHandler<GetTradesQuery, BaseResponse<IEnumerable<TradeDto>>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public GetTradesQueryHandler(ITradeRepository tradeRepository, IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<IEnumerable<TradeDto>>> Handle(GetTradesQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<IEnumerable<TradeDto>>.Unauthorized("User not found.");

            var trades = await _tradeRepository.GetListAsyncUntracked<Trade>(
                filter: t =>
                    t.UserId == user.Id &&
                    (request.Ticker == null || t.Ticker.ToLower().Contains(request.Ticker.ToLower())) &&
                    (request.OptionType == null || t.OptionType == request.OptionType) &&
                    (request.Strategy == null || t.Strategy == request.Strategy) &&
                    (request.FromDate == null || t.TradeDate >= request.FromDate) &&
                    (request.ToDate == null || t.TradeDate <= request.ToDate),
                pageSize: request.PageSize,
                pageNumber: request.PageNumber);

            return BaseResponse<IEnumerable<TradeDto>>.Ok(trades.Select(t => t.ToDto()));
        }
    }
}
