using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class SeedTradesCommandHandler : ICommandHandler<SeedTradesCommand, BaseResponse<int>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        private static readonly string[] Tickers = { "AAPL", "TSLA", "MSFT", "NVDA", "META", "AMZN", "GOOGL", "SPY", "QQQ" };
        private static readonly string[] OptionTypes = { "Call", "Put" };
        private static readonly string[] Strategies = { "Breakout + Retest", "Consolidation Zone" };
        private static readonly string[] AllViolationTags = { "Revenge Trade", "FOMO Entry", "Oversized Position", "Early Exit", "Late Exit", "Chased Entry", "No Clear Setup", "Broke Profit Target", "Overtraded" };

        public SeedTradesCommandHandler(ITradeRepository tradeRepository, IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<int>> Handle(SeedTradesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<int>.Unauthorized("User not found.");

            var random = new Random();
            var trades = new List<Trade>();

            for (int i = 0; i < 25; i++)
            {
                var entryPrice = (decimal)Math.Round(random.NextDouble() * 10 + 1, 2);
                var isWin = random.NextDouble() > 0.4; // 60% win rate
                var exitPrice = isWin
                    ? entryPrice * (decimal)(1 + random.NextDouble() * 0.5)   // up to 50% gain
                    : entryPrice * (decimal)(1 - random.NextDouble() * 0.4);  // up to 40% loss
                exitPrice = Math.Round(exitPrice, 2);

                var quantity = random.Next(1, 5);
                var pnl = (exitPrice - entryPrice) * quantity * 100;

                var tagCount = random.Next(0, 4); // 0-3 tags
                var violationTags = AllViolationTags.OrderBy(_ => random.Next()).Take(tagCount).ToList();
                var disciplineScore = tagCount switch { 0 => 100, 1 => 70, 2 => 40, _ => 10 };
                var aiScore = random.Next(40, 100);

                trades.Add(new Trade
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Ticker = Tickers[random.Next(Tickers.Length)],
                    OptionType = OptionTypes[random.Next(OptionTypes.Length)],
                    Strategy = Strategies[random.Next(Strategies.Length)],
                    EntryPrice = entryPrice,
                    ExitPrice = exitPrice,
                    Quantity = quantity,
                    Dte = random.Next(7, 45),
                    TradeDate = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                    Pnl = pnl,
                    Notes = "Seeded test trade",
                    ViolationTags = violationTags,
                    AiScore = aiScore,
                    AiFeedback = "Seeded — no real AI analysis",
                    DisciplineScore = disciplineScore,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var trade in trades)
                await _tradeRepository.AddAsync(trade);

            return BaseResponse<int>.Created(trades.Count, $"Seeded {trades.Count} trades.");
        }
    }
}
