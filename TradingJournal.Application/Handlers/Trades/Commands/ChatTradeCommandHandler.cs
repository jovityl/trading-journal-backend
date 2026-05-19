using TradingJournal.Application.Interfaces;
using TradingJournal.Contract.Abstraction;
using TradingJournal.Contract.Common;
using TradingJournal.Domain.Entities;
using TradingJournal.Domain.IRepository;
using static TradingJournal.Contract.Message.Trades.Commands;

namespace TradingJournal.Application.Handlers.Trades.Commands
{
    public class ChatTradeCommandHandler : ICommandHandler<ChatTradeCommand, BaseResponse<string>>
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatServiceRouter _chatRouter;
        private readonly IChatModerationService _moderationService;
        private readonly IPromptService _promptService;
        private readonly ITokenUsageService _tokenUsageService;
        private readonly ITradeMessageRepository _messageRepository;

        public ChatTradeCommandHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            IChatServiceRouter chatRouter,
            IChatModerationService moderationService,
            IPromptService promptService,
            ITokenUsageService tokenUsageService,
            ITradeMessageRepository messageRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _chatRouter = chatRouter;
            _moderationService = moderationService;
            _promptService = promptService;
            _tokenUsageService = tokenUsageService;
            _messageRepository = messageRepository;
        }

        public async Task<BaseResponse<string>> Handle(ChatTradeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(filter: u => u.Auth0Id == request.Auth0Id);
            if (user is null)
                return BaseResponse<string>.Unauthorized("User not found.");

            var trade = await _tradeRepository.GetOneAsyncUntracked<Trade>(
                filter: t => t.Id == request.TradeId && t.UserId == user.Id);
            if (trade is null)
                return BaseResponse<string>.NotFound("Trade not found.");

            // Persist the latest user message
            var latestUserMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
            if (!string.IsNullOrWhiteSpace(latestUserMessage))
            {
                await _messageRepository.AddAsync(new TradeMessage
                {
                    Id = Guid.NewGuid(),
                    TradeId = trade.Id,
                    Role = "user",
                    Content = latestUserMessage,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Cheap pre-check: classify the latest user message
            if (!string.IsNullOrWhiteSpace(latestUserMessage))
            {
                var moderation = await _moderationService.IsOnTopicAsync(latestUserMessage, cancellationToken);
                await _tokenUsageService.RecordAsync(user.Id, "moderation", moderation.Usage, cancellationToken);

                if (!moderation.IsOnTopic)
                {
                    var cannedReply = "I can only help with questions about trading or this specific trade. " +
                        "Try asking about your entry timing, strategy, discipline, or what to do differently next time.";

                    await _messageRepository.AddAsync(new TradeMessage
                    {
                        Id = Guid.NewGuid(),
                        TradeId = trade.Id,
                        Role = "assistant",
                        Content = cannedReply,
                        Model = "moderation",
                        CreatedAt = DateTime.UtcNow
                    });

                    return BaseResponse<string>.Ok(cannedReply);
                }
            }

            var template = await _promptService.GetAsync(PromptKeys.ChatSystem, cancellationToken);
            var systemPrompt = FillTradePlaceholders(template, trade);

            // Load chart image if present
            Stream? imageStream = null;
            string? imageContentType = null;
            if (!string.IsNullOrEmpty(trade.ChartScreenshotUrl))
            {
                var fileName = trade.ChartScreenshotUrl.Replace("/uploads/", "");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileName);
                if (File.Exists(filePath))
                {
                    imageStream = File.OpenRead(filePath);
                    var ext = Path.GetExtension(filePath).ToLower();
                    imageContentType = ext switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".webp" => "image/webp",
                        _ => "image/png"
                    };
                }
            }

            try
            {
                // Cost control: only send last 6 messages (3 exchanges) to the AI.
                // All messages still saved in DB and shown in UI — just not sent to the model.
                var messages = request.Messages
                    .TakeLast(6)
                    .Select(m => new ChatMessage { Role = m.Role, Content = m.Content })
                    .ToList();

                var chatService = _chatRouter.Resolve(request.Model);
                var chatResult = await chatService.ChatAsync(
                    systemPrompt,
                    imageStream,
                    imageContentType,
                    messages,
                    request.Model,
                    cancellationToken);

                await _tokenUsageService.RecordAsync(user.Id, "chat", chatResult.Usage, cancellationToken);

                // Persist assistant reply
                await _messageRepository.AddAsync(new TradeMessage
                {
                    Id = Guid.NewGuid(),
                    TradeId = trade.Id,
                    Role = "assistant",
                    Content = chatResult.Reply,
                    Model = request.Model,
                    CreatedAt = DateTime.UtcNow
                });

                return BaseResponse<string>.Ok(chatResult.Reply);
            }
            finally
            {
                imageStream?.Dispose();
            }
        }

        private static string FillTradePlaceholders(string template, Trade trade)
        {
            return template
                .Replace("{ticker}", trade.Ticker)
                .Replace("{optionType}", trade.OptionType)
                .Replace("{strategy}", trade.Strategy)
                .Replace("{entryPrice}", trade.EntryPrice.ToString())
                .Replace("{exitPrice}", trade.ExitPrice.ToString())
                .Replace("{underlyingEntry}", trade.UnderlyingEntryPrice.HasValue ? $"${trade.UnderlyingEntryPrice}" : "not provided")
                .Replace("{underlyingExit}", trade.UnderlyingExitPrice.HasValue ? $"${trade.UnderlyingExitPrice}" : "not provided")
                .Replace("{quantity}", trade.Quantity.ToString())
                .Replace("{dte}", trade.Dte.ToString())
                .Replace("{pnl}", trade.Pnl.ToString())
                .Replace("{disciplineScore}", trade.DisciplineScore.ToString())
                .Replace("{notes}", trade.Notes ?? "(none)")
                .Replace("{aiFeedback}", trade.AiFeedback ?? "(none)");
        }
    }
}
