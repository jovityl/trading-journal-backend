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
        private readonly IChatService _chatService;

        public ChatTradeCommandHandler(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            IChatService chatService)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _chatService = chatService;
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

            var systemPrompt = BuildSystemPrompt(trade);

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
                var messages = request.Messages
                    .Select(m => new ChatMessage { Role = m.Role, Content = m.Content })
                    .ToList();

                var reply = await _chatService.ChatAsync(
                    systemPrompt,
                    imageStream,
                    imageContentType,
                    messages,
                    cancellationToken);

                return BaseResponse<string>.Ok(reply);
            }
            finally
            {
                imageStream?.Dispose();
            }
        }

        private static string BuildSystemPrompt(Trade trade) => $@"
You are an experienced options trading coach helping the user reflect on this specific trade.

Trade details:
- Ticker: {trade.Ticker}
- Option type: {trade.OptionType}
- Strategy: {trade.Strategy}
- Option entry premium: ${trade.EntryPrice}
- Option exit premium: ${trade.ExitPrice}
- Underlying entry: {(trade.UnderlyingEntryPrice.HasValue ? $"${trade.UnderlyingEntryPrice}" : "not provided")}
- Underlying exit: {(trade.UnderlyingExitPrice.HasValue ? $"${trade.UnderlyingExitPrice}" : "not provided")}
- Quantity: {trade.Quantity}
- DTE: {trade.Dte}
- P&L: ${trade.Pnl}
- Discipline score: {trade.DisciplineScore}/100
- User's notes: {trade.Notes ?? "(none)"}
- Previous AI analysis: {trade.AiFeedback ?? "(none)"}

The chart screenshot for this trade is attached (if available).

Answer the user's questions clearly and conversationally. Reference specific things from the chart and trade data.
Be concise — give actionable, practical advice without long-winded explanations.
This is an OPTIONS trade, so manage discretionarily (no hard stops needed).
";
    }
}
