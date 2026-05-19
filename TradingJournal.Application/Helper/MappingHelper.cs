using TradingJournal.Contract.DTOs;
using TradingJournal.Domain.Entities;

namespace TradingJournal.Application.Helper
{
    public static class MappingHelper
    {
        public static TradeDto ToDto(this Trade entity)
        {
            return new TradeDto
            {
                Id = entity.Id,
                Ticker = entity.Ticker,
                OptionType = entity.OptionType,
                Strategy = entity.Strategy,
                EntryPrice = entity.EntryPrice,
                ExitPrice = entity.ExitPrice,
                UnderlyingEntryPrice = entity.UnderlyingEntryPrice,
                UnderlyingExitPrice = entity.UnderlyingExitPrice,
                Quantity = entity.Quantity,
                Dte = entity.Dte,
                TradeDate = entity.TradeDate,
                Pnl = entity.Pnl,
                Notes = entity.Notes,
                IbkrScreenshotUrl = entity.IbkrScreenshotUrl,
                ChartScreenshotUrl = entity.ChartScreenshotUrl,
                AiScore = entity.AiScore,
                AiFeedback = entity.AiFeedback,
                EntryQuality = entity.EntryQuality,
                ExitQuality = entity.ExitQuality,
                RiskManagement = entity.RiskManagement,
                PlanAdherence = entity.PlanAdherence,
                TickedScore = entity.TickedScore,
                DisciplineScore = entity.DisciplineScore,
                CreatedAt = entity.CreatedAt
            };
        }

        public static UserDto ToDto(this User entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                Email = entity.Email,
                DisplayName = entity.DisplayName,
                DailyLossLimit = entity.DailyLossLimit,
                DailyProfitTarget = entity.DailyProfitTarget
            };
        }
    }
}
