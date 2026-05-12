using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;
using SoftFocusBackend.Users.Application.ACL.Services;

namespace SoftFocusBackend.Tests.Users.Application;

public class UserCommandServiceTest
{
    [Fact]
    public async Task HandleCreateUserAsync_WhenValidGeneralUser_ShouldReturnUser()
    {
        // Arrange (Preparación)
        // 1. Creamos "Mocks" (simuladores) de todas las dependencias
        var mockUserRepository = new Mock<IUserRepository>();
        var mockUserDomainService = new Mock<IUserDomainService>();
        var mockAuthNotificationService = new Mock<IAuthNotificationService>();
        var mockCloudinaryService = new Mock<ICloudinaryImageService>();
        var mockEmailService = new Mock<IGenericEmailService>();
        var mockLogger = new Mock<ILogger<UserCommandService>>();

        // 2. Instanciamos el servicio pasándole los Mocks
        var service = new UserCommandService(
            mockUserRepository.Object,
            mockUserDomainService.Object,
            mockAuthNotificationService.Object,
            mockCloudinaryService.Object,
            mockEmailService.Object,
            mockLogger.Object
        );

        // 3. Preparamos los datos de entrada (El comando)
        // Nota: Ajusta los parámetros según cómo esté estructurado tu Record/Class CreateUserCommand
        var command = new CreateUserCommand(
            email: "giancarlo@test.com",
            passwordHash: "hashed123",
            fullName: "Giancarlo Test",
            userType: UserType.General
        );

        // 4. Preparamos lo que queremos que devuelva nuestro Mock del DomainService
        var expectedUser = new User { 
            Email = "giancarlo@test.com", 
            FullName = "Giancarlo Test", 
            UserType = UserType.General 
        };

        mockUserDomainService
            .Setup(s => s.IsEmailUniqueAsync(command.Email, It.IsAny<string?>()))
            .ReturnsAsync(true);

        mockUserDomainService
            .Setup(s => s.CreateUserAsync(command.Email, command.PasswordHash, command.FullName, command.UserType))
            .ReturnsAsync(expectedUser);

        // Act (Ejecución)
        var result = await service.HandleCreateUserAsync(command);

        // Assert (Verificación)
        Assert.NotNull(result); // Verificamos que el usuario creado no sea nulo
        Assert.Equal("giancarlo@test.com", result.Email); // Verificamos que el email coincida
        
        // Verificamos que el método AddAsync del repositorio fue llamado exactamente 1 vez
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once); 
    }
}