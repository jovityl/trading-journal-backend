using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Admin
{
    public static class Queries
    {
        public record GetPromptsQuery() : IQuery<BaseResponse<List<PromptDto>>>;
        public record GetUsageQuery() : IQuery<BaseResponse<UsageSummaryDto>>;
    }
}
