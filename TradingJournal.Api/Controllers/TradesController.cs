using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using static TradingJournal.Contract.Message.Trades.Commands;
using static TradingJournal.Contract.Message.Trades.Queries;
using static TradingJournal.Contract.Message.Trades.Request;

namespace TradingJournal.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TradesController : ControllerBase
    {
        private readonly ISender _sender;

        public TradesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<BaseResponse<IEnumerable<TradeDto>>> GetTrades(
            [FromQuery] string? ticker,
            [FromQuery] string? optionType,
            [FromQuery] string? strategy,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? pageSize,
            [FromQuery] int? pageNumber)
        {
            var query = new GetTradesQuery(ticker, optionType, strategy, fromDate, toDate, pageSize, pageNumber);
            return await _sender.Send(query);
        }

        [HttpGet("{id:guid}")]
        public async Task<BaseResponse<TradeDto>> GetTradeById([FromRoute] Guid id)
        {
            var query = new GetTradeByIdQuery(id);
            return await _sender.Send(query);
        }

        [HttpPost]
        public async Task<BaseResponse<TradeDto>> CreateTrade([FromForm] CreateTradeRequest request)
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var command = new CreateTradeCommand(
                request.Ticker,
                request.OptionType,
                request.Strategy,
                request.EntryPrice,
                request.ExitPrice,
                request.Quantity,
                request.Dte,
                request.TradeDate,
                request.Notes,
                request.HasStopLoss,
                request.HasProfitTarget,
                request.HasPositionSizing,
                request.HasAppropriateDte,
                request.IbkrScreenshot,
                request.ChartScreenshot,
                auth0Id);

            return await _sender.Send(command);
        }
        [HttpDelete("{id:guid}")]
        public async Task<BaseResponse<bool>> DeleteTrade([FromRoute] Guid id)
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var command = new DeleteTradeCommand(id, auth0Id);
            return await _sender.Send(command);
        }
    }
}
