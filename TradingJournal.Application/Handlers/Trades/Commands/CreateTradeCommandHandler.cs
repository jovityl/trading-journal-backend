using TradingJournal.Application.Interfaces;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class CreateTradeCommandHandler : ICommandHandler<CreateTradeCommand, BaseResponse<TradeDto>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStorageService _storageService;
        private readonly IAiScoringService _aiScoringService;

        public CreateTradeCommandHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            IStorageService storageService,
            IAiScoringService aiScoringService)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _storageService = storageService;
            _aiScoringService = aiScoringService;
        }

        public async Task<BaseResponse<TradeDto>> Handle(CreateTradeCommand request, CancellationToken cancellationToken)
        {
            // Find user by Auth0Id
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<TradeDto>.Unauthorized("User not found.");

            // Calculate P&L
            var pnl = (request.ExitPrice - request.EntryPrice) * request.Quantity * 100;

            // Calculate ticked score (5 points each, max 20)
            var tickedScore = 0;
            if (request.HasStopLoss) tickedScore += 5;
            if (request.HasProfitTarget) tickedScore += 5;
            if (request.HasPositionSizing) tickedScore += 5;
            if (request.HasAppropriateDte) tickedScore += 5;

            // Upload IBKR screenshot if provided
            string? ibkrScreenshotUrl = null;
            if (request.IbkrScreenshot is not null)
            {
                var stream = request.IbkrScreenshot.OpenReadStream();
                ibkrScreenshotUrl = await _storageService.UploadFileAsync(
                    stream,
                    request.IbkrScreenshot.FileName,
                    request.IbkrScreenshot.ContentType,
                    cancellationToken);
            }

            // Upload chart screenshot + AI scoring if provided
            string? chartScreenshotUrl = null;
            int aiScore = 0;
            string? aiFeedback = null;

            if (request.ChartScreenshot is not null)
            {
                // Read into memory once so we can both upload and analyze
                using var memoryStream = new MemoryStream();
                await request.ChartScreenshot.OpenReadStream().CopyToAsync(memoryStream, cancellationToken);

                // Upload
                memoryStream.Position = 0;
                chartScreenshotUrl = await _storageService.UploadFileAsync(
                    memoryStream,
                    request.ChartScreenshot.FileName,
                    request.ChartScreenshot.ContentType,
                    cancellationToken);

                // AI scoring
                memoryStream.Position = 0;
                var aiResult = await _aiScoringService.ScoreChartAsync(
                    memoryStream,
                    request.ChartScreenshot.ContentType,
                    request.Strategy,
                    request.EntryPrice,
                    request.ExitPrice,
                    request.OptionType,
                    request.Dte,
                    cancellationToken);

                aiScore = aiResult.Score;
                aiFeedback = aiResult.Feedback;
            }

            var disciplineScore = tickedScore + aiScore; // out of 100

            var trade = new Trade
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Ticker = request.Ticker,
                OptionType = request.OptionType,
                Strategy = request.Strategy,
                EntryPrice = request.EntryPrice,
                ExitPrice = request.ExitPrice,
                Quantity = request.Quantity,
                Dte = request.Dte,
                TradeDate = request.TradeDate,
                Pnl = pnl,
                Notes = request.Notes,
                IbkrScreenshotUrl = ibkrScreenshotUrl,
                ChartScreenshotUrl = chartScreenshotUrl,
                HasStopLoss = request.HasStopLoss,
                HasProfitTarget = request.HasProfitTarget,
                HasPositionSizing = request.HasPositionSizing,
                HasAppropriateDte = request.HasAppropriateDte,
                TickedScore = tickedScore,
                AiScore = aiScore,
                AiFeedback = aiFeedback,
                DisciplineScore = disciplineScore,
                CreatedAt = DateTime.UtcNow
            };

            await _tradeRepository.AddAsync(trade);

            return BaseResponse<TradeDto>.Created(new TradeDto
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
