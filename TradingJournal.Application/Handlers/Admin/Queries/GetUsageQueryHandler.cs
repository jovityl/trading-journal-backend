using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Admin.Queries;

namespace TradingJournal.Application.Handlers.Admin.Queries
{
    public class GetUsageQueryHandler : IQueryHandler<GetUsageQuery, BaseResponse<UsageSummaryDto>>
    {
        private readonly ITokenUsageRepository _tokenUsageRepository;
        private readonly IUserRepository _userRepository;

        public GetUsageQueryHandler(ITokenUsageRepository tokenUsageRepository, IUserRepository userRepository)
        {
            _tokenUsageRepository = tokenUsageRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<UsageSummaryDto>> Handle(GetUsageQuery request, CancellationToken cancellationToken)
        {
            var usages = (await _tokenUsageRepository.GetListAsyncUntracked<TokenUsage>()).ToList();
            var users = (await _userRepository.GetListAsync()).ToList();
            var userById = users.ToDictionary(u => u.Id, u => u.Email);

            var perUser = usages
                .GroupBy(u => u.UserId)
                .Select(g => new UserUsageDto
                {
                    UserId = g.Key,
                    Email = userById.TryGetValue(g.Key, out var email) ? email : "(unknown)",
                    TotalCalls = g.Count(),
                    TotalInputTokens = g.Sum(u => u.InputTokens + u.CacheCreationTokens + u.CacheReadTokens),
                    TotalOutputTokens = g.Sum(u => u.OutputTokens),
                    TotalCost = g.Sum(u => u.Cost)
                })
                .OrderByDescending(p => p.TotalCost)
                .ToList();

            var recentCalls = usages
                .OrderByDescending(u => u.CreatedAt)
                .Take(50)
                .Select(u => new UsageCallDto
                {
                    Id = u.Id,
                    Email = userById.TryGetValue(u.UserId, out var email) ? email : "(unknown)",
                    Endpoint = u.Endpoint,
                    Model = u.Model,
                    InputTokens = u.InputTokens,
                    OutputTokens = u.OutputTokens,
                    CacheReadTokens = u.CacheReadTokens,
                    Cost = u.Cost,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            var summary = new UsageSummaryDto
            {
                TotalCalls = usages.Count,
                TotalInputTokens = usages.Sum(u => u.InputTokens + u.CacheCreationTokens + u.CacheReadTokens),
                TotalOutputTokens = usages.Sum(u => u.OutputTokens),
                TotalCost = usages.Sum(u => u.Cost),
                PerUser = perUser,
                RecentCalls = recentCalls
            };

            return BaseResponse<UsageSummaryDto>.Ok(summary);
        }
    }
}
