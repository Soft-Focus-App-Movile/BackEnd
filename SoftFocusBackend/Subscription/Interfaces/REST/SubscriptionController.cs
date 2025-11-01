using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Subscription.Application.Commands;
using SoftFocusBackend.Subscription.Application.DTOs;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Application.Services;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using System.Security.Claims;

namespace SoftFocusBackend.Subscription.Interfaces.REST;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionCommandService _commandService;
    private readonly ISubscriptionQueryService _queryService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionCommandService commandService,
        ISubscriptionQueryService queryService,
        ILogger<SubscriptionController> logger)
    {
        _commandService = commandService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's subscription
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMySubscription()
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var subscription = await _queryService.GetSubscriptionByUserIdAsync(
                new GetSubscriptionByUserIdQuery { UserId = userId });

            if (subscription == null)
            {
                return NotFound(new { message = "Subscription not found" });
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get usage statistics for current user
    /// </summary>
    [HttpGet("usage")]
    public async Task<IActionResult> GetUsageStats()
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var stats = await _queryService.GetUsageStatsAsync(
                new GetUsageStatsQuery { UserId = userId });

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check access to a specific feature
    /// </summary>
    [HttpGet("check-access/{featureType}")]
    public async Task<IActionResult> CheckFeatureAccess(FeatureType featureType)
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var accessResponse = await _queryService.CheckFeatureAccessAsync(
                new CheckFeatureAccessQuery
                {
                    UserId = userId,
                    FeatureType = featureType
                });

            if (!accessResponse.HasAccess)
            {
                return StatusCode(429, accessResponse); // 429 Too Many Requests
            }

            return Ok(accessResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature access");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a checkout session to upgrade to Pro
    /// </summary>
    [HttpPost("upgrade/checkout")]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var response = await _commandService.CreateProCheckoutSessionAsync(userId, request);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating checkout session");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription([FromBody] bool cancelImmediately = false)
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var subscription = await _commandService.CancelSubscriptionAsync(
                new CancelSubscriptionCommand
                {
                    UserId = userId,
                    CancelImmediately = cancelImmediately
                });

            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation cancelling subscription");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Track feature usage (internal endpoint, typically called by other services)
    /// </summary>
    [HttpPost("track-usage")]
    public async Task<IActionResult> TrackUsage([FromBody] TrackFeatureUsageCommand command)
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            // Ensure user can only track their own usage
            if (command.UserId != userId)
            {
                return Forbid();
            }

            await _commandService.TrackFeatureUsageAsync(command);

            return Ok(new { message = "Usage tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking usage");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
