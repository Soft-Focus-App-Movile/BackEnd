using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.ValueObjects;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SoftFocusBackend.Notification.Application.Internal.CommandServices
{
    public class SendNotificationCommandService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPreferenceRepository _preferenceRepository;
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly INotificationSchedulingService _schedulingService;
        private readonly IDeliveryOptimizationService _optimizationService;

        public SendNotificationCommandService(
            INotificationRepository notificationRepository,
            INotificationPreferenceRepository preferenceRepository,
            INotificationTemplateRepository templateRepository,
            INotificationSchedulingService schedulingService,
            IDeliveryOptimizationService optimizationService)
        {
            _notificationRepository = notificationRepository;
            _preferenceRepository = preferenceRepository;
            _templateRepository = templateRepository;
            _schedulingService = schedulingService;
            _optimizationService = optimizationService;
        }

        // Método para enviar nueva notificación
        public async Task<Domain.Model.Aggregates.Notification> HandleAsync(SendNotificationCommand command)
        {
            var preference = await _preferenceRepository.GetByUserAndTypeAsync(command.UserId, command.Type);
            
            if (preference != null && !preference.IsEnabled)
                throw new InvalidOperationException($"User has disabled {command.Type} notifications");

            var deliveryMethod = command.DeliveryMethod ??
                (preference?.DeliveryMethod ??
                await _optimizationService.DetermineOptimalMethodAsync(command.UserId, command.Type));

            var scheduledAt = command.ScheduledAt ??
                await _schedulingService.CalculateOptimalDeliveryTimeAsync(command.UserId, command.Type);

            var notification = new Domain.Model.Aggregates.Notification
            {
                UserId = command.UserId,
                Type = command.Type,
                Title = command.Title,
                Content = command.Content,
                Priority = command.Priority,
                DeliveryMethod = deliveryMethod,
                Status = DeliveryStatus.Pending.ToString(),
                ScheduledAt = scheduledAt,
                Metadata = command.Metadata ?? new Dictionary<string, object>()
            };

            await _notificationRepository.CreateAsync(notification);
            
            return notification;
        }

        // 🔹 Nuevo método: actualizar notificación existente
        public async Task UpdateAsync(Domain.Model.Aggregates.Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _notificationRepository.UpdateAsync(notification.Id, notification);
        }

    }
}
