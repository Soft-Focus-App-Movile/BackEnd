using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Application.Services;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using System.Security.Claims;

namespace SoftFocusBackend.Subscription.Infrastructure.Filters;

/// <summary>
/// Action filter to validate subscription feature access before executing endpoint
/// Usage: [RequireFeatureAccess(FeatureType.AiChatMessage)]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireFeatureAccessAttribute : Attribute, IAsyncActionFilter
{
    public FeatureType FeatureType { get; }
    public bool TrackUsage { get; set; } = false;

    public RequireFeatureAccessAttribute(FeatureType featureType)
    {
        FeatureType = featureType;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var queryService = context.HttpContext.RequestServices
            .GetService<ISubscriptionQueryService>();

        if (queryService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var userId = context.HttpContext.User.FindFirstValue("user_id");
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var accessResponse = await queryService.CheckFeatureAccessAsync(
                new CheckFeatureAccessQuery
                {
                    UserId = userId,
                    FeatureType = FeatureType
                });

            if (!accessResponse.HasAccess)
            {
                context.Result = new ObjectResult(new
                {
                    message = accessResponse.DenialReason,
                    upgradeMessage = accessResponse.UpgradeMessage,
                    currentUsage = accessResponse.CurrentUsage,
                    limit = accessResponse.Limit
                })
                {
                    StatusCode = 429 // Too Many Requests
                };
                return;
            }

            // If access is granted and TrackUsage is true, track after execution
            if (TrackUsage)
            {
                var executedContext = await next();

                // Only track if the request was successful
                if (executedContext.Exception == null &&
                    context.HttpContext.Response.StatusCode >= 200 &&
                    context.HttpContext.Response.StatusCode < 300)
                {
                    var commandService = context.HttpContext.RequestServices
                        .GetService<ISubscriptionCommandService>();

                    if (commandService != null)
                    {
                        await commandService.TrackFeatureUsageAsync(
                            new Application.Commands.TrackFeatureUsageCommand
                            {
                                UserId = userId,
                                FeatureType = FeatureType
                            });
                    }
                }

                return;
            }

            await next();
        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireFeatureAccessAttribute>>();

            logger?.LogError(ex, "Error checking feature access for user: {UserId}, feature: {FeatureType}",
                userId, FeatureType);

            context.Result = new StatusCodeResult(500);
        }
    }
}
