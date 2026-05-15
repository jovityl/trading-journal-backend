using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using static TradingJournal.Contract.Message.Users.Commands;
using static TradingJournal.Contract.Message.Users.Request;

namespace TradingJournal.Api.Controllers
{
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
            var command = new CreateUserCommand(request.Auth0Id, request.Email, request.DisplayName);
            return await _sender.Send(command);
        }

        [HttpPut("limits")]
        public async Task<BaseResponse<UserDto>> UpdateLimits([FromBody] UpdateUserLimitsRequest request)
        {
            // TODO: replace with actual Auth0Id from JWT token after auth is set up
            var auth0Id = "auth0|test123";
            var command = new UpdateUserLimitsCommand(auth0Id, request.DailyLossLimit, request.DailyProfitTarget);
            return await _sender.Send(command);
        }
    }
}
