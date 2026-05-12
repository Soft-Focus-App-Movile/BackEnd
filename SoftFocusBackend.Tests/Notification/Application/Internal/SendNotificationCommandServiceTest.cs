using Moq;
using Xunit;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Services;

namespace SoftFocusBackend.Tests.Notification.Application;

public class SendNotificationCommandServiceTest
{
    [Fact]
    public async Task HandleAsync_WhenNotificationsAreNotDisabled_ShouldCreateNotification()
    {
        // Arrange (Preparación)
        // 1. Creamos "Mocks" para todas las dependencias del servicio
        var mockNotificationRepo = new Mock<INotificationRepository>();
        var mockPreferenceRepo = new Mock<INotificationPreferenceRepository>();
        var mockTemplateRepo = new Mock<INotificationTemplateRepository>();
        var mockSchedulingService = new Mock<INotificationSchedulingService>();
        var mockOptimizationService = new Mock<IDeliveryOptimizationService>();

        // 2. Instanciamos el servicio con los Mocks
        var service = new SendNotificationCommandService(
            mockNotificationRepo.Object,
            mockPreferenceRepo.Object,
            mockTemplateRepo.Object,
            mockSchedulingService.Object,
            mockOptimizationService.Object
        );

        // 3. Preparamos el comando de entrada
        // (Nota: si tu record SendNotificationCommand pide los datos en otro orden, ajústalos aquí)
        var command = new SendNotificationCommand(
            UserId: "user_12345",
            Type: "THERAPY_REMINDER",
            Title: "Tu sesión de terapia",
            Content: "Tu sesión empieza en 15 minutos.",
            Priority: "High",
            DeliveryMethod: "Push",
            ScheduledAt: DateTime.UtcNow,
            Metadata: new Dictionary<string, object>()
        );

        // 4. Configuramos el comportamiento simulado
        // Simulamos que al buscar preferencias, no retorna nada (es decir, no están bloqueadas)
        // Usamos dynamic o el tipo exacto si lo conoces para el retorno nulo
        mockPreferenceRepo
            .Setup(r => r.GetByUserAndTypeAsync(command.UserId, command.Type))
            .ReturnsAsync((SoftFocusBackend.Notification.Domain.Model.Aggregates.NotificationPreference?)null);

        // Act (Ejecución)
        var result = await service.HandleAsync(command);

        // Assert (Verificación)
        Assert.NotNull(result); // Validamos que nos devuelve una entidad
        Assert.Equal("user_12345", result.UserId); // Validamos que es para el usuario correcto
        Assert.Equal("Tu sesión de terapia", result.Title); // Validamos el título
        Assert.Equal("Pending", result.Status); // Validamos que nace con estado Pendiente
        
        // ¡Lo más importante! Validamos que el método CreateAsync de la base de datos se llamó 1 sola vez
        mockNotificationRepo.Verify(r => r.CreateAsync(It.IsAny<SoftFocusBackend.Notification.Domain.Model.Aggregates.Notification>()), Times.Once);
    }
}