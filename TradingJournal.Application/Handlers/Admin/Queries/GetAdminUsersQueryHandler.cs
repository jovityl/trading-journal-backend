using TradingJournal.Application.Interfaces;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Admin.Queries;

namespace TradingJournal.Application.Handlers.Admin.Queries
{
    public class GetAdminUsersQueryHandler : IQueryHandler<GetAdminUsersQuery, BaseResponse<List<AdminUserDto>>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly ITokenUsageRepository _tokenUsageRepository;
        private readonly IAdminSettings _adminSettings;

        public GetAdminUsersQueryHandler(
            IUserRepository userRepository,
            ITradeRepository tradeRepository,
            ITokenUsageRepository tokenUsageRepository,
            IAdminSettings adminSettings)
        {
            _userRepository = userRepository;
            _tradeRepository = tradeRepository;
            _tokenUsageRepository = tokenUsageRepository;
            _adminSettings = adminSettings;
        }

        public async Task<BaseResponse<List<AdminUserDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        {
            var users = (await _userRepository.GetListAsync()).ToList();
            var trades = (await _tradeRepository.GetListAsyncUntracked<Trade>()).ToList();
            var usages = (await _tokenUsageRepository.GetListAsyncUntracked<TokenUsage>()).ToList();

            // Pre-group for fast lookup
            var tradesByUser = trades.GroupBy(t => t.UserId).ToDictionary(g => g.Key, g => g.ToList());
            var usageByUser = usages.GroupBy(u => u.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var result = users
                .Select(u =>
                {
                    var userTrades = tradesByUser.TryGetValue(u.Id, out var t) ? t : new List<Trade>();
                    var userUsages = usageByUser.TryGetValue(u.Id, out var us) ? us : new List<TokenUsage>();

                    return new AdminUserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        DisplayName = u.DisplayName,
                        IsAdmin = _adminSettings.IsAdmin(u.Email),
                        CreatedAt = u.CreatedAt,
                        TradeCount = userTrades.Count,
                        TotalPnl = userTrades.Sum(t => t.Pnl),
                        TotalAiCalls = userUsages.Count,
                        TotalAiCost = userUsages.Sum(u => u.Cost)
                    };
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            return BaseResponse<List<AdminUserDto>>.Ok(result);
        }
    }
}
