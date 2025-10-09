using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Model.ValueObjects;
using SoftFocusBackend.Notification.Infrastructure.ExternalServices;

namespace SoftFocusBackend.Notification.Infrastructure.BackgroundServices;

public class NotificationDeliveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDeliveryService> _logger;

    public NotificationDeliveryService(
        IServiceProvider serviceProvider,
        ILogger<NotificationDeliveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var fcmService = scope.ServiceProvider.GetRequiredService<FirebaseFCMService>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailNotificationService>();

                // Get scheduled notifications
                var notifications = await notificationRepository.GetScheduledNotificationsAsync(DateTime.UtcNow);

                foreach (var notification in notifications)
                {
                    try
                    {
                        var delivered = false;

                        switch (notification.DeliveryMethod)
                        {
                            case "Push":
                                // Get device token and send push notification
                                delivered = await fcmService.SendPushNotificationAsync(
                                    "device_token", // Would get from user service
                                    notification.Title,
                                    notification.Content,
                                    notification.Metadata
                                );
                                break;

                            case "Email":
                                // Get email and send
                                delivered = await emailService.SendEmailNotificationAsync(
                                    "user@email.com", // Would get from user service
                                    notification.Title,
                                    notification.Content
                                );
                                break;

                            case "InApp":
                                // Mark as delivered for in-app notifications
                                delivered = true;
                                break;
                        }

                        if (delivered)
                        {
                            notification.MarkAsDelivered();
                        }
                        else if (notification.ShouldRetry())
                        {
                            notification.MarkAsFailed("Delivery failed");
                            notification.ScheduledAt = DateTime.UtcNow.AddMinutes(5 * notification.RetryCount);
                        }

                        await notificationRepository.UpdateAsync(notification.Id, notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing notification {notification.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification delivery service");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}