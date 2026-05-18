using TradingJournal.Application.Interfaces;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Admin.Commands;

namespace TradingJournal.Application.Handlers.Admin.Commands
{
    public class UpdatePromptCommandHandler : ICommandHandler<UpdatePromptCommand, BaseResponse<PromptDto>>
    {
        private readonly IPromptRepository _promptRepository;
        private readonly IPromptService _promptService;

        public UpdatePromptCommandHandler(IPromptRepository promptRepository, IPromptService promptService)
        {
            _promptRepository = promptRepository;
            _promptService = promptService;
        }

        public async Task<BaseResponse<PromptDto>> Handle(UpdatePromptCommand request, CancellationToken cancellationToken)
        {
            var prompt = await _promptRepository.GetOneAsync(filter: p => p.Key == request.Key);
            if (prompt is null)
                return BaseResponse<PromptDto>.NotFound($"Prompt '{request.Key}' not found.");

            prompt.Content = request.Content;
            prompt.UpdatedAt = DateTime.UtcNow;
            await _promptRepository.UpdateAsync(prompt);

            // Invalidate cache so next AI call picks up the new prompt
            _promptService.Invalidate();

            return BaseResponse<PromptDto>.Ok(new PromptDto
            {
                Key = prompt.Key,
                Content = prompt.Content,
                UpdatedAt = prompt.UpdatedAt
            });
        }
    }
}
