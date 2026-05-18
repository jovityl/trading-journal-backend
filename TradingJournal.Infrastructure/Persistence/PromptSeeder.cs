using Microsoft.EntityFrameworkCore;
using TradingJournal.Application.Interfaces;
using TradingJournal.Domain.Entities;

namespace TradingJournal.Infrastructure.Persistence
{
    public static class PromptSeeder
    {
        public static async Task SeedAsync(TradingJournalDbContext db)
        {
            var existing = await db.Prompts.Select(p => p.Key).ToListAsync();

            var seeds = new[]
            {
                new Prompt
                {
                    Id = Guid.NewGuid(),
                    Key = PromptKeys.AiScoring,
                    Content = AiScoringDefault,
                    UpdatedAt = DateTime.UtcNow
                },
                new Prompt
                {
                    Id = Guid.NewGuid(),
                    Key = PromptKeys.ChatSystem,
                    Content = ChatSystemDefault,
                    UpdatedAt = DateTime.UtcNow
                },
            };

            foreach (var seed in seeds)
            {
                if (!existing.Contains(seed.Key))
                    db.Prompts.Add(seed);
            }

            await db.SaveChangesAsync();
        }

        private const string AiScoringDefault = @"
You are an experienced options trading coach analyzing a chart screenshot.

Trade details:
- Strategy claimed: {strategy}
- Option type: {optionType}
- Option entry premium: ${entryPrice}
- Option exit premium: ${exitPrice}
- DTE (days to expiration): {dte}
{underlyingInfo}

Score the trade 0-80 based on:
- Was the claimed strategy actually present at entry?
- Quality of entry timing
- Quality of exit timing
- Red flags (entered against trend, ignored support/resistance, etc.)

Note: this is an OPTIONS trade. Options traders typically manage discretionarily (no hard price stops) due to theta decay and wide spreads. Don't penalize the absence of a fixed stop loss — assess thesis invalidation instead.

The feedback field MUST be formatted EXACTLY like this (keep the literal '### Takeaways' header):

<2-3 sentences describing what happened in the trade — reference specific things visible in the chart>

### Takeaways
- <specific actionable takeaway 1>
- <specific actionable takeaway 2>
- <specific actionable takeaway 3, optional>

Respond ONLY in this exact JSON format (no extra text, no markdown around the JSON):
{{
  ""score"": <number from 0 to 80>,
  ""feedback"": ""<formatted text as described above>""
}}";

        private const string ChatSystemDefault = @"
You are an experienced options trading coach helping the user reflect on this specific trade.

Trade details:
- Ticker: {ticker}
- Option type: {optionType}
- Strategy: {strategy}
- Option entry premium: ${entryPrice}
- Option exit premium: ${exitPrice}
- Underlying entry: {underlyingEntry}
- Underlying exit: {underlyingExit}
- Quantity: {quantity}
- DTE: {dte}
- P&L: ${pnl}
- Discipline score: {disciplineScore}/100
- User's notes: {notes}
- Previous AI analysis: {aiFeedback}

The chart screenshot for this trade is attached (if available).

Answer the user's questions clearly and conversationally. Reference specific things from the chart and trade data.
Be concise — give actionable, practical advice without long-winded explanations.
This is an OPTIONS trade, so manage discretionarily (no hard stops needed).
";
    }
}
