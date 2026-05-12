// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: UserCommandService
//  Estrategia: se mockean todos los colaboradores (repositorios, servicios de
//              dominio, notificaciones, Cloudinary, email, logger) y se verifica
//              el comportamiento del servicio de forma aislada.
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;
using SoftFocusBackend.Users.Application.ACL.Services;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Users;

public class UserCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserRepository>              _userRepoMock          = new();
    private readonly Mock<IUserDomainService>           _domainServiceMock     = new();
    private readonly Mock<IAuthNotificationService>     _notificationMock      = new();
    private readonly Mock<ICloudinaryImageService>      _cloudinaryMock        = new();
    private readonly Mock<IGenericEmailService>         _emailServiceMock      = new();
    private readonly Mock<ILogger<UserCommandService>>  _loggerMock            = new();

    private readonly UserCommandService _sut;

    public UserCommandServiceTests()
    {
        _sut = new UserCommandService(
            _userRepoMock.Object,
            _domainServiceMock.Object,
            _notificationMock.Object,
            _cloudinaryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static User BuildUser(
        string id       = "user-001",
        string email    = "ana@test.com",
        string fullName = "Ana García",
        UserType type   = UserType.General)
    {
        var user = new User
        {
            Email    = email,
            FullName = fullName,
            UserType = type,
            IsActive = true
        };
        // Asignar Id mediante reflexión (BaseEntity lo tiene privado)
        SetPrivateId(user, id);
        return user;
    }

    private static void SetPrivateId(object obj, string id)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty("Id",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (prop?.CanWrite == true) { prop.SetValue(obj, id); return; }

            var field = type.GetField("<Id>k__BackingField",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            if (field != null) { field.SetValue(obj, id); return; }

            type = type.BaseType;
        }
    }

    private static CreateUserCommand BuildCreateCommand(
        string email    = "ana@test.com",
        string password = "hashed-password",
        string fullName = "Ana García",
        UserType type   = UserType.General) =>
        new(email, password, fullName, type);

    // ─── HandleCreateUserAsync — Escenarios felices ───────────────────────

    [Fact]
    public async Task HandleCreateUserAsync_ValidGeneralUser_PersistsAndReturnsUser()
    {
        // Arrange
        var user    = BuildUser();
        var command = BuildCreateCommand();

        _domainServiceMock
            .Setup(d => d.IsEmailUniqueAsync(command.Email, null))
            .ReturnsAsync(true);

        _domainServiceMock
            .Setup(d => d.CreateUserAsync(command.Email, command.PasswordHash, command.FullName, command.UserType))
            .ReturnsAsync(user);

        _userRepoMock
            .Setup(r => r.AddAsync(user))
            .Returns(Task.CompletedTask);

        _notificationMock
            .Setup(n => n.NotifyUserCreatedAsync(user.Id, user.Email, user.UserType.ToString()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleCreateUserAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FullName.Should().Be(command.FullName);
    }

    [Fact]
    public async Task HandleCreateUserAsync_ValidUser_CallsRepositoryAddOnce()
    {
        // Arrange
        var user    = BuildUser();
        var command = BuildCreateCommand();

        _domainServiceMock.Setup(d => d.IsEmailUniqueAsync(command.Email, null)).ReturnsAsync(true);
        _domainServiceMock.Setup(d => d.CreateUserAsync(command.Email, command.PasswordHash, command.FullName, command.UserType))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.AddAsync(user)).Returns(Task.CompletedTask);
        _notificationMock.Setup(n => n.NotifyUserCreatedAsync(user.Id, user.Email, user.UserType.ToString()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.HandleCreateUserAsync(command);

        // Assert
        _userRepoMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task HandleCreateUserAsync_ValidUser_NotifiesUserCreation()
    {
        // Arrange
        var user    = BuildUser();
        var command = BuildCreateCommand();

        _domainServiceMock.Setup(d => d.IsEmailUniqueAsync(command.Email, null)).ReturnsAsync(true);
        _domainServiceMock.Setup(d => d.CreateUserAsync(command.Email, command.PasswordHash, command.FullName, command.UserType))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.AddAsync(user)).Returns(Task.CompletedTask);
        _notificationMock.Setup(n => n.NotifyUserCreatedAsync(user.Id, user.Email, user.UserType.ToString()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.HandleCreateUserAsync(command);

        // Assert
        _notificationMock.Verify(
            n => n.NotifyUserCreatedAsync(user.Id, user.Email, user.UserType.ToString()),
            Times.Once);
    }

    // ─── HandleCreateUserAsync — Escenarios de error ──────────────────────

    [Fact]
    public async Task HandleCreateUserAsync_DuplicateEmail_ReturnsNull()
    {
        // Arrange
        var command = BuildCreateCommand();

        _domainServiceMock
            .Setup(d => d.IsEmailUniqueAsync(command.Email, null))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.HandleCreateUserAsync(command);

        // Assert
        result.Should().BeNull();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleCreateUserAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange — email sin '@' invalida el command
        var command = new CreateUserCommand("not-an-email", "hash", "Ana García", UserType.General);

        // Act
        var result = await _sut.HandleCreateUserAsync(command);

        // Assert
        result.Should().BeNull();
        _domainServiceMock.Verify(d => d.IsEmailUniqueAsync(It.IsAny<string>(), null), Times.Never);
    }

    [Theory]
    [InlineData("", "hash", "Ana García")]
    [InlineData("ana@test.com", "", "Ana García")]
    [InlineData("ana@test.com", "hash", "")]
    public async Task HandleCreateUserAsync_MissingRequiredFields_ReturnsNull(
        string email, string passwordHash, string fullName)
    {
        // Arrange
        CreateUserCommand command;
        try
        {
            command = new CreateUserCommand(email, passwordHash, fullName, UserType.General);
        }
        catch (ArgumentNullException)
        {
            // El constructor mismo lanza para nulos — comportamiento correcto
            return;
        }

        // Act
        var result = await _sut.HandleCreateUserAsync(command);

        // Assert
        result.Should().BeNull();
    }

    // ─── HandleUpdateUserProfileAsync — Escenarios felices ───────────────

    [Fact]
    public async Task HandleUpdateUserProfileAsync_ValidCommand_UpdatesAndReturnsUser()
    {
        // Arrange
        var user    = BuildUser();
        var command = new UpdateUserProfileCommand(
            userId:   "user-001",
            fullName: "Ana María García",
            firstName: "Ana María",
            lastName:  "García");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("user-001"))
            .ReturnsAsync(user);

        _notificationMock
            .Setup(n => n.NotifyUserUpdatedAsync(user.Id, user.Email))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleUpdateUserProfileAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Ana María García");
    }

    [Fact]
    public async Task HandleUpdateUserProfileAsync_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var user    = BuildUser();
        var command = new UpdateUserProfileCommand("user-001", "Ana García Actualizada");

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _notificationMock.Setup(n => n.NotifyUserUpdatedAsync(user.Id, user.Email)).Returns(Task.CompletedTask);

        // Act
        await _sut.HandleUpdateUserProfileAsync(command);

        // Assert
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateUserProfileAsync_ValidCommand_NotifiesUpdate()
    {
        // Arrange
        var user    = BuildUser();
        var command = new UpdateUserProfileCommand("user-001", "Ana García");

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _notificationMock.Setup(n => n.NotifyUserUpdatedAsync(user.Id, user.Email)).Returns(Task.CompletedTask);

        // Act
        await _sut.HandleUpdateUserProfileAsync(command);

        // Assert
        _notificationMock.Verify(n => n.NotifyUserUpdatedAsync(user.Id, user.Email), Times.Once);
    }

    // ─── HandleUpdateUserProfileAsync — Escenarios de error ──────────────

    [Fact]
    public async Task HandleUpdateUserProfileAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var command = new UpdateUserProfileCommand("unknown-id", "Ana García");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("unknown-id"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.HandleUpdateUserProfileAsync(command);

        // Assert
        result.Should().BeNull();
        _notificationMock.Verify(n => n.NotifyUserUpdatedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleUpdateUserProfileAsync_EmptyFullName_ReturnsNull()
    {
        // Arrange — FullName vacío invalida el command
        var command = new UpdateUserProfileCommand("user-001", "");

        // Act
        var result = await _sut.HandleUpdateUserProfileAsync(command);

        // Assert
        result.Should().BeNull();
        _userRepoMock.Verify(r => r.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── HandleDeleteUserAsync — Escenarios felices ───────────────────────

    [Fact]
    public async Task HandleDeleteUserAsync_SoftDelete_DeactivatesUserAndReturnsTrue()
    {
        // Arrange
        var user    = BuildUser();
        var command = new DeleteUserCommand("user-001", hardDelete: false);

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _domainServiceMock.Setup(d => d.CanUserBeDeletedAsync("user-001")).ReturnsAsync(true);
        _notificationMock.Setup(n => n.NotifyUserDeletedAsync(user.Id, user.Email)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleDeleteUserAsync(command);

        // Assert
        result.Should().BeTrue();
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleDeleteUserAsync_HardDelete_RemovesUserAndReturnsTrue()
    {
        // Arrange
        var user    = BuildUser();
        var command = new DeleteUserCommand("user-001", hardDelete: true);

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _domainServiceMock.Setup(d => d.CanUserBeDeletedAsync("user-001")).ReturnsAsync(true);
        _notificationMock.Setup(n => n.NotifyUserDeletedAsync(user.Id, user.Email)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleDeleteUserAsync(command);

        // Assert
        result.Should().BeTrue();
        _userRepoMock.Verify(r => r.Remove(user), Times.Once);
        _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleDeleteUserAsync_AnyDelete_NotifiesDeletion()
    {
        // Arrange
        var user    = BuildUser();
        var command = new DeleteUserCommand("user-001");

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _domainServiceMock.Setup(d => d.CanUserBeDeletedAsync("user-001")).ReturnsAsync(true);
        _notificationMock.Setup(n => n.NotifyUserDeletedAsync(user.Id, user.Email)).Returns(Task.CompletedTask);

        // Act
        await _sut.HandleDeleteUserAsync(command);

        // Assert
        _notificationMock.Verify(n => n.NotifyUserDeletedAsync(user.Id, user.Email), Times.Once);
    }

    // ─── HandleDeleteUserAsync — Escenarios de error ──────────────────────

    [Fact]
    public async Task HandleDeleteUserAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var command = new DeleteUserCommand("unknown-id");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("unknown-id"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.HandleDeleteUserAsync(command);

        // Assert
        result.Should().BeFalse();
        _notificationMock.Verify(n => n.NotifyUserDeletedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleDeleteUserAsync_UserCannotBeDeleted_ReturnsFalse()
    {
        // Arrange — regla de negocio: usuario con relaciones activas no puede eliminarse
        var user    = BuildUser();
        var command = new DeleteUserCommand("user-001");

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);
        _domainServiceMock.Setup(d => d.CanUserBeDeletedAsync("user-001")).ReturnsAsync(false);

        // Act
        var result = await _sut.HandleDeleteUserAsync(command);

        // Assert
        result.Should().BeFalse();
        _userRepoMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Never);
        _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleDeleteUserAsync_InvalidCommand_ReturnsFalse()
    {
        // Arrange — UserId vacío invalida el command; el constructor lanza
        var act = () => new DeleteUserCommand(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
