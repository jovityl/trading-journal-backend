using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingJournal.Contract.Common;
using TradingJournal.Contract.DTOs;
using static TradingJournal.Contract.Message.Admin.Commands;
using static TradingJournal.Contract.Message.Admin.Queries;

namespace TradingJournal.Api.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("prompts")]
        public async Task<BaseResponse<List<PromptDto>>> GetPrompts()
        {
            return await _sender.Send(new GetPromptsQuery());
        }

        [HttpGet("usage")]
        public async Task<BaseResponse<UsageSummaryDto>> GetUsage()
        {
            return await _sender.Send(new GetUsageQuery());
        }

        [HttpGet("users")]
        public async Task<BaseResponse<List<AdminUserDto>>> GetUsers()
        {
            return await _sender.Send(new GetAdminUsersQuery());
        }

        public record UpdatePromptBody(string Content);

        [HttpPut("prompts/{key}")]
        public async Task<BaseResponse<PromptDto>> UpdatePrompt([FromRoute] string key, [FromBody] UpdatePromptBody body)
        {
            return await _sender.Send(new UpdatePromptCommand(key, body.Content));
        }
    }
}
