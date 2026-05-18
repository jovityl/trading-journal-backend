using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;

namespace TradingJournal.Contract.Message.Admin
{
    public static class Commands
    {
        public record UpdatePromptCommand(string Key, string Content) : ICommand<BaseResponse<PromptDto>>;
    }
}
