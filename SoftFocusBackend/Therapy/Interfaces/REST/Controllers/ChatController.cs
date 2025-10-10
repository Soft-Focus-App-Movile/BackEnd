﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly SendChatMessageCommandService _sendService;
        private readonly ChatHistoryQueryService _historyService;
        private readonly SignalRChatService _signalRService;

        public ChatController(
            SendChatMessageCommandService sendService,
            ChatHistoryQueryService historyService,
            SignalRChatService signalRService)
        {
            _sendService = sendService;
            _historyService = historyService;
            _signalRService = signalRService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request)
        {
            var command = new SendChatMessageCommand
            {
                RelationshipId = request.RelationshipId,
                SenderId = GetCurrentUserId(),
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType
            };

            var message = await _sendService.Handle(command);

            // Send real-time notification to the receiver
            await _signalRService.SendMessageAsync(request.ReceiverId, new
            {
                MessageId = message.Id,
                Content = message.Content.Value,
                SenderId = message.SenderId,
                Timestamp = message.Timestamp
            });

            return Ok(new { MessageId = message.Id });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(string relationshipId, int page = 1, int size = 20)
        {
            var query = new GetChatHistoryQuery { RelationshipId = relationshipId, Page = page, Size = size };
            var history = await _historyService.Handle(query);
            return Ok(history);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("user_id")?.Value;
        }
    }
}