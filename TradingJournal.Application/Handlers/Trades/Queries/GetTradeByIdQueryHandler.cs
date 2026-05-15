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

        public GetTradeByIdQueryHandler(ITradeRepository tradeRepository)
        {
            _tradeRepository = tradeRepository;
        }

        public async Task<BaseResponse<TradeDto>> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
        {
            var trade = await _tradeRepository.GetOneAsyncUntracked<Trade>(
                filter: t => t.Id == request.Id);

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
