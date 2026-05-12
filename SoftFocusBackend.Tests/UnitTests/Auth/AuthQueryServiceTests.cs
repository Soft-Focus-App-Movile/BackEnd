// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: AuthQueryService
//  Estrategia: se mockean IUserContextService e ILogger y se verifica
//              el comportamiento del servicio de forma aislada.
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Application.Internal.QueryServices;
using SoftFocusBackend.Auth.Domain.Model.Queries;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tests.UnitTests.Auth;

public class AuthQueryServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserContextService>         _userContextMock = new();
    private readonly Mock<ILogger<AuthQueryService>>   _loggerMock      = new();

    private readonly AuthQueryService _sut;

    public AuthQueryServiceTests()
    {
        _sut = new AuthQueryService(_userContextMock.Object, _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static AuthenticatedUser BuildUser(string id = "user-001", string role = "General") =>
        new(id, "Ana García", "ana@test.com", role);

    // ─── HandleGetCurrentUserAsync — Escenarios felices ──────────────────

    [Fact]
    public async Task HandleGetCurrentUserAsync_ValidUserId_ReturnsAuthenticatedUser()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetCurrentUserQuery("user-001");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("user-001"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetCurrentUserAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("user-001");
        result.Email.Should().Be("ana@test.com");
        result.Role.Should().Be("General");
    }

    [Fact]
    public async Task HandleGetCurrentUserAsync_ValidUserId_CallsContextServiceOnce()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetCurrentUserQuery("user-001");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("user-001"))
            .ReturnsAsync(user);

        // Act
        await _sut.HandleGetCurrentUserAsync(query);

        // Assert
        _userContextMock.Verify(u => u.GetUserByIdAsync("user-001"), Times.Once);
    }

    [Fact]
    public async Task HandleGetCurrentUserAsync_PsychologistUser_ReturnsPsychologistRole()
    {
        // Arrange
        var user  = BuildUser("psy-001", "Psychologist");
        var query = new GetCurrentUserQuery("psy-001");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("psy-001"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetCurrentUserAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.IsPsychologist().Should().BeTrue();
        result.IsGeneral().Should().BeFalse();
    }

    [Fact]
    public async Task HandleGetCurrentUserAsync_AdminUser_ReturnsAdminRole()
    {
        // Arrange
        var user  = BuildUser("admin-001", "Admin");
        var query = new GetCurrentUserQuery("admin-001");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("admin-001"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetCurrentUserAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.IsAdmin().Should().BeTrue();
    }

    // ─── HandleGetCurrentUserAsync — Escenarios de error ─────────────────

    [Fact]
    public async Task HandleGetCurrentUserAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var query = new GetCurrentUserQuery("nonexistent-id");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("nonexistent-id"))
            .ReturnsAsync((AuthenticatedUser?)null);

        // Act
        var result = await _sut.HandleGetCurrentUserAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleGetCurrentUserAsync_ContextServiceThrows_ReturnsNull()
    {
        // Arrange
        var query = new GetCurrentUserQuery("user-001");

        _userContextMock
            .Setup(u => u.GetUserByIdAsync("user-001"))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _sut.HandleGetCurrentUserAsync(query);

        // Assert — el servicio captura la excepción y devuelve null
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleGetCurrentUserAsync_ValidQuery_IsValidReturnsTrue()
    {
        // Arrange
        var query = new GetCurrentUserQuery("any-user-id");

        // Assert — la query con userId válido siempre es válida
        query.IsValid().Should().BeTrue();
        query.UserId.Should().Be("any-user-id");
    }
}
