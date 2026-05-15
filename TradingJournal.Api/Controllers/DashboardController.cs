using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using static TradingJournal.Contract.Message.Dashboard.Queries;

namespace TradingJournal.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ISender _sender;

        public DashboardController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<BaseResponse<DashboardDto>> GetDashboard()
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var query = new GetDashboardQuery(auth0Id);
            return await _sender.Send(query);
        }
    }
}
