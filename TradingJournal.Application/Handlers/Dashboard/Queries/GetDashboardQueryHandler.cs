using TradingJournal.Application.Helper;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Dashboard.Queries;

namespace TradingJournal.Application.Handlers.Dashboard.Queries
{
    public class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, BaseResponse<DashboardDto>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;

        public GetDashboardQueryHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<DashboardDto>.Unauthorized("User not found.");

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Get all trades for this user
            var allTrades = await _tradeRepository.GetListAsyncUntracked<Trade>(
                filter: t => t.UserId == user.Id);

            var todayTrades = allTrades.Where(t => t.TradeDate.Date == today).ToList();
            var monthTrades = allTrades.Where(t => t.TradeDate >= startOfMonth).ToList();

            // Total P&L (all-time)
            var totalPnl = allTrades.Sum(t => t.Pnl);

            // Today's P&L
            var todayPnl = todayTrades.Sum(t => t.Pnl);

            // Monthly P&L
            var monthlyPnl = monthTrades.Sum(t => t.Pnl);

            // Win rate
            var totalTrades = allTrades.Count();
            var winningTrades = allTrades.Count(t => t.Pnl > 0);
            var winRate = totalTrades > 0 ? (double)winningTrades / totalTrades * 100 : 0;

            // Average discipline score
            var avgScore = totalTrades > 0 ? allTrades.Average(t => t.DisciplineScore) : 0;

            // Daily limit alerts (only if limits are actually set)
            var isDailyLossLimitHit = user.DailyLossLimit < 0 && todayPnl <= user.DailyLossLimit;
            var isDailyProfitTargetHit = user.DailyProfitTarget > 0 && todayPnl >= user.DailyProfitTarget;

            // P&L chart (last 30 days) — daily values
            var pnlChart = allTrades
                .Where(t => t.TradeDate >= today.AddDays(-30))
                .GroupBy(t => t.TradeDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new PnlChartDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Pnl = g.Sum(t => t.Pnl)
                }).ToList();

            // Equity curve — cumulative P&L across ALL trades (history of account growth)
            var equityCurve = new List<PnlChartDto>();
            decimal running = 0;
            foreach (var group in allTrades.GroupBy(t => t.TradeDate.Date).OrderBy(g => g.Key))
            {
                running += group.Sum(t => t.Pnl);
                equityCurve.Add(new PnlChartDto
                {
                    Date = group.Key.ToString("yyyy-MM-dd"),
                    Pnl = running
                });
            }

            // Score chart (last 30 days)
            var scoreChart = allTrades
                .Where(t => t.TradeDate >= today.AddDays(-30))
                .GroupBy(t => t.TradeDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ScoreChartDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    AverageScore = g.Average(t => t.DisciplineScore)
                }).ToList();

            // Recent trades (last 5 logged)
            var recentTrades = allTrades
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => t.ToDto())
                .ToList();

            return BaseResponse<DashboardDto>.Ok(new DashboardDto
            {
                TotalPnl = totalPnl,
                TodayPnl = todayPnl,
                MonthlyPnl = monthlyPnl,
                WinRate = Math.Round(winRate, 2),
                AverageDisciplineScore = Math.Round(avgScore, 2),
                IsDailyLossLimitHit = isDailyLossLimitHit,
                IsDailyProfitTargetHit = isDailyProfitTargetHit,
                DailyLossLimit = user.DailyLossLimit,
                DailyProfitTarget = user.DailyProfitTarget,
                PnlChart = pnlChart,
                EquityCurve = equityCurve,
                ScoreChart = scoreChart,
                RecentTrades = recentTrades
            });
        }
    }
}
