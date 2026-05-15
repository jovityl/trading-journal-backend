using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using static TradingJournal.Contract.Message.Users.Commands;
using static TradingJournal.Contract.Message.Users.Request;

namespace TradingJournal.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<BaseResponse<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var command = new CreateUserCommand(auth0Id, request.Email, request.DisplayName);
            return await _sender.Send(command);
        }

        [HttpPut("limits")]
        public async Task<BaseResponse<UserDto>> UpdateLimits([FromBody] UpdateUserLimitsRequest request)
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var command = new UpdateUserLimitsCommand(auth0Id, request.DailyLossLimit, request.DailyProfitTarget);
            return await _sender.Send(command);
        }
    }
}
