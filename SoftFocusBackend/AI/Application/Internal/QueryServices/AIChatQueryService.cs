using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Queries;
using SoftFocusBackend.AI.Domain.Repositories;

namespace SoftFocusBackend.AI.Application.Internal.QueryServices;

public class AIChatQueryService
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly ILogger<AIChatQueryService> _logger;

    public AIChatQueryService(
        IChatSessionRepository sessionRepository,
        ILogger<AIChatQueryService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ChatSession>> HandleGetUserChatHistoryAsync(GetUserChatHistoryQuery query)
    {
        try
        {
            _logger.LogInformation("Getting chat history for user {UserId}", query.UserId);
            return await _sessionRepository.GetUserSessionsAsync(
                query.UserId,
                query.FromDate,
                query.ToDate,
                query.PageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for user {UserId}", query.UserId);
            throw;
        }
    }

    public async Task<ChatSession?> HandleGetChatSessionAsync(GetChatSessionQuery query)
    {
        try
        {
            _logger.LogInformation("Getting chat session {SessionId}", query.SessionId);
            return await _sessionRepository.GetByIdAsync(query.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat session {SessionId}", query.SessionId);
            throw;
        }
    }

    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId, int limit = 50)
    {
        try
        {
            _logger.LogInformation("Getting messages for session {SessionId}", sessionId);
            return await _sessionRepository.GetSessionMessagesAsync(sessionId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for session {SessionId}", sessionId);
            throw;
        }
    }
}
