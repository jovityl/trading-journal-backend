using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Admin.Queries;

namespace TradingJournal.Application.Handlers.Admin.Queries
{
    public class GetPromptsQueryHandler : IQueryHandler<GetPromptsQuery, BaseResponse<List<PromptDto>>>
    {
        private readonly IPromptRepository _promptRepository;

        public GetPromptsQueryHandler(IPromptRepository promptRepository)
        {
            _promptRepository = promptRepository;
        }

        public async Task<BaseResponse<List<PromptDto>>> Handle(GetPromptsQuery request, CancellationToken cancellationToken)
        {
            var prompts = await _promptRepository.GetListAsyncUntracked<Prompt>();
            var result = prompts
                .OrderBy(p => p.Key)   // stable alphabetical order so UI doesn't jump after edits
                .Select(p => new PromptDto
                {
                    Key = p.Key,
                    Content = p.Content,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

            return BaseResponse<List<PromptDto>>.Ok(result);
        }
    }
}
