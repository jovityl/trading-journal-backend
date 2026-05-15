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

            return BaseResponse<TradeDto>.Ok(new TradeDto
            {
                Id = trade.Id,
                Ticker = trade.Ticker,
                OptionType = trade.OptionType,
                Strategy = trade.Strategy,
                EntryPrice = trade.EntryPrice,
                ExitPrice = trade.ExitPrice,
                Quantity = trade.Quantity,
                Dte = trade.Dte,
                TradeDate = trade.TradeDate,
                Pnl = trade.Pnl,
                Notes = trade.Notes,
                IbkrScreenshotUrl = trade.IbkrScreenshotUrl,
                ChartScreenshotUrl = trade.ChartScreenshotUrl,
                AiScore = trade.AiScore,
                AiFeedback = trade.AiFeedback,
                HasStopLoss = trade.HasStopLoss,
                HasProfitTarget = trade.HasProfitTarget,
                HasPositionSizing = trade.HasPositionSizing,
                HasAppropriateDte = trade.HasAppropriateDte,
                TickedScore = trade.TickedScore,
                DisciplineScore = trade.DisciplineScore,
                CreatedAt = trade.CreatedAt
            });
        }
    }
}
