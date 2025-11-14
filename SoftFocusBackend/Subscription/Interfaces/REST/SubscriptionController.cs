using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Subscription.Application.Commands;
using SoftFocusBackend.Subscription.Application.DTOs;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Application.Services;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Subscription.Interfaces.REST;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
[Produces("application/json")]
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

    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Get user subscription",
        Description = "Retrieves the current subscription details for the authenticated user. Auto-creates a Basic subscription if none exists.",
        OperationId = "GetMySubscription",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                // Auto-create subscription for existing users without one
                _logger.LogWarning("Subscription not found for user: {UserId}. Creating one automatically.", userId);

                var userTypeStr = User.FindFirstValue("user_type") ?? "General";
                var userType = Enum.Parse<SoftFocusBackend.Users.Domain.Model.ValueObjects.UserType>(userTypeStr);

                var command = new CreateBasicSubscriptionCommand
                {
                    UserId = userId,
                    UserType = userType
                };

                subscription = await _commandService.CreateBasicSubscriptionAsync(command);

                _logger.LogInformation("Auto-created Basic subscription for user: {UserId}", userId);
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("usage")]
    [SwaggerOperation(
        Summary = "Get usage statistics",
        Description = "Retrieves usage statistics for AI features (chat, facial analysis) for the authenticated user.",
        OperationId = "GetUsageStats",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsageStats()
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            // Check if subscription exists first
            var subscription = await _queryService.GetSubscriptionByUserIdAsync(
                new GetSubscriptionByUserIdQuery { UserId = userId });

            if (subscription == null)
            {
                // Auto-create subscription for existing users without one
                _logger.LogWarning("Subscription not found for user: {UserId}. Creating one automatically.", userId);

                var userTypeStr = User.FindFirstValue("user_type") ?? "General";
                var userType = Enum.Parse<SoftFocusBackend.Users.Domain.Model.ValueObjects.UserType>(userTypeStr);

                var command = new CreateBasicSubscriptionCommand
                {
                    UserId = userId,
                    UserType = userType
                };

                await _commandService.CreateBasicSubscriptionAsync(command);

                _logger.LogInformation("Auto-created Basic subscription for user: {UserId}", userId);
            }

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

    [HttpGet("check-access/{featureType}")]
    [SwaggerOperation(
        Summary = "Check feature access",
        Description = "Checks if the user has access to a specific feature based on their subscription plan and usage limits.",
        OperationId = "CheckFeatureAccess",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpPost("upgrade/checkout")]
    [SwaggerOperation(
        Summary = "Create Pro upgrade checkout",
        Description = "Creates a Stripe checkout session to upgrade the user's subscription to Pro plan.",
        OperationId = "CreateCheckoutSession",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpPost("cancel")]
    [SwaggerOperation(
        Summary = "Cancel subscription",
        Description = "Cancels the user's subscription. Can be immediate or at the end of the billing period.",
        OperationId = "CancelSubscription",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpPost("track-usage")]
    [SwaggerOperation(
        Summary = "Track feature usage",
        Description = "Records usage of a specific feature for billing and limit enforcement. Typically called internally by other services.",
        OperationId = "TrackUsage",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    [HttpPost("initialize")]
    [SwaggerOperation(
        Summary = "Initialize subscription",
        Description = "Creates a Basic subscription for existing users who don't have one yet. Helper endpoint for migration.",
        OperationId = "InitializeSubscription",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitializeSubscription()
    {
        try
        {
            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var userTypeStr = User.FindFirstValue("user_type") ?? "General";
            var userType = Enum.Parse<SoftFocusBackend.Users.Domain.Model.ValueObjects.UserType>(userTypeStr);

            var command = new CreateBasicSubscriptionCommand
            {
                UserId = userId,
                UserType = userType
            };

            var subscription = await _commandService.CreateBasicSubscriptionAsync(command);

            _logger.LogInformation("Initialized subscription for user: {UserId}", userId);

            return Ok(new
            {
                message = "Subscription initialized successfully",
                subscription
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing subscription");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("checkout/success")]
    [SwaggerOperation(
        Summary = "Handle successful checkout",
        Description = "Processes a successful Stripe checkout session and upgrades the subscription to PRO",
        OperationId = "HandleCheckoutSuccess",
        Tags = new[] { "Subscriptions" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleCheckoutSuccess([FromQuery] string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(new { message = "Session ID is required" });
            }

            var userId = User.FindFirstValue("user_id")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var subscription = await _commandService.HandleSuccessfulCheckoutAsync(sessionId);

            if (subscription == null)
            {
                return NotFound(new { message = "Checkout session not found or already processed" });
            }

            _logger.LogInformation("Successfully processed checkout for user: {UserId}, session: {SessionId}", userId, sessionId);

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling checkout success for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred while processing the checkout" });
        }
    }
}
