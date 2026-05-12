// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: AuthCommandService
//  Estrategia: se mockean todos los colaboradores (IUserContextService,
//              TokenService, IOAuthTempTokenService, IServiceProvider, ILogger)
//              y se verifica el comportamiento del servicio de forma aislada.
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Application.Internal.CommandServices;
using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Auth.Infrastructure.OAuth.Services;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Configuration;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Services;

namespace SoftFocusBackend.Tests.UnitTests.Auth;

public class AuthCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserContextService>           _userContextMock       = new();
    private readonly Mock<IOAuthTempTokenService>        _oauthTempTokenMock    = new();
    private readonly Mock<IServiceProvider>              _serviceProviderMock   = new();
    private readonly Mock<IServiceScope>                 _serviceScopeMock      = new();
    private readonly Mock<IServiceScopeFactory>          _scopeFactoryMock      = new();
    private readonly Mock<ILogger<AuthCommandService>>   _loggerMock            = new();

    private readonly TokenService         _tokenService;
    private readonly AuthCommandService   _sut;

    public AuthCommandServiceTests()
    {
        // TokenService necesita opciones reales con clave suficientemente larga
        var tokenSettings = Options.Create(new TokenSettings
        {
            SecretKey              = "supersecret-test-key-at-least-32-chars-long!",
            Issuer                 = "SoftFocus",
            Audience               = "SoftFocusUsers",
            ExpirationHours        = 1,
            ValidateIssuerSigningKey = true,
            ValidateIssuer         = true,
            ValidateAudience       = true,
            ValidateLifetime       = true,
            RequireExpirationTime  = true,
            RequireSignedTokens    = true,
            ClockSkewMinutes       = 5
        });

        var tokenLogger = Mock.Of<ILogger<TokenService>>();
        _tokenService = new TokenService(tokenSettings, tokenLogger);

        // Configurar IServiceProvider para el Task.Run de UpdateLastLogin
        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock
            .Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);
        _serviceProviderMock
            .Setup(p => p.GetService(typeof(IUserContextService)))
            .Returns(_userContextMock.Object);

        _sut = new AuthCommandService(
            _userContextMock.Object,
            _tokenService,
            _serviceProviderMock.Object,
            _oauthTempTokenMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static AuthenticatedUser BuildGeneralUser(string? id = null) =>
        new(id ?? "user-001", "Ana García", "ana@test.com", "General");

    private static AuthenticatedUser BuildPsychologistUser(bool isVerified = true) =>
        new("psy-001", "Dr. López", "doctor@test.com", "Psychologist", isVerified: isVerified);

    // ─── HandleSignInAsync — Escenarios felices ───────────────────────────

    [Fact]
    public async Task HandleSignInAsync_ValidCredentials_ReturnsAuthToken()
    {
        // Arrange
        var user    = BuildGeneralUser();
        var command = new SignInCommand("ana@test.com", "Password1!");

        _userContextMock
            .Setup(u => u.AuthenticateUserAsync("ana@test.com", "Password1!"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleSignInAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task HandleSignInAsync_ValidCredentials_TokenContainsUserClaims()
    {
        // Arrange
        var user    = BuildGeneralUser();
        var command = new SignInCommand("ana@test.com", "Password1!");

        _userContextMock
            .Setup(u => u.AuthenticateUserAsync("ana@test.com", "Password1!"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleSignInAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.GetUserId().Should().Be(user.Id);
        result.GetUserEmail().Should().Be(user.Email);
        result.GetUserRole().Should().Be(user.Role);
    }

    [Fact]
    public async Task HandleSignInAsync_VerifiedPsychologist_ReturnsAuthToken()
    {
        // Arrange
        var user    = BuildPsychologistUser(isVerified: true);
        var command = new SignInCommand("doctor@test.com", "Password1!");

        _userContextMock
            .Setup(u => u.AuthenticateUserAsync("doctor@test.com", "Password1!"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleSignInAsync(command);

        // Assert
        result.Should().NotBeNull();
    }

    // ─── HandleSignInAsync — Escenarios de error ──────────────────────────

    [Fact]
    public async Task HandleSignInAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange — email sin '@' invalida el command
        var command = new SignInCommand("not-an-email", "Password1!");

        // Act
        var result = await _sut.HandleSignInAsync(command);

        // Assert
        result.Should().BeNull();
        _userContextMock.Verify(u => u.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleSignInAsync_WrongCredentials_ReturnsNull()
    {
        // Arrange
        var command = new SignInCommand("ana@test.com", "WrongPass1!");

        _userContextMock
            .Setup(u => u.AuthenticateUserAsync("ana@test.com", "WrongPass1!"))
            .ReturnsAsync((AuthenticatedUser?)null);

        // Act
        var result = await _sut.HandleSignInAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleSignInAsync_UnverifiedPsychologist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user    = BuildPsychologistUser(isVerified: false);
        var command = new SignInCommand("doctor@test.com", "Password1!");

        _userContextMock
            .Setup(u => u.AuthenticateUserAsync("doctor@test.com", "Password1!"))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.HandleSignInAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Theory]
    [InlineData("", "Password1!")]
    [InlineData("ana@test.com", "")]
    public void HandleSignInAsync_EmptyCredentials_ThrowsArgumentException(string email, string password)
    {
        // LoginCredentials valida en construcción y lanza antes de llegar al servicio
        var act = () => new SignInCommand(email, password);
        act.Should().Throw<ArgumentException>();
    }

    // ─── HandleRegisterGeneralUserAsync — Escenarios felices ─────────────

    [Fact]
    public async Task HandleRegisterGeneralUserAsync_ValidCommand_ReturnsUserId()
    {
        // Arrange
        var user    = BuildGeneralUser("new-user-123");
        var command = new RegisterGeneralUserCommand(
            firstName: "Ana",
            lastName:  "García",
            email:     "ana@test.com",
            password:  "Secure1!",
            acceptsPrivacyPolicy: true);

        _userContextMock
            .Setup(u => u.CreateUserAsync("ana@test.com", "Secure1!", "Ana García", "General",
                null, null, null, null, null, null, null, null, null, null))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleRegisterGeneralUserAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("new-user-123");
    }

    [Fact]
    public async Task HandleRegisterGeneralUserAsync_UserCreationFails_ReturnsNull()
    {
        // Arrange
        var command = new RegisterGeneralUserCommand("Ana", "García", "ana@test.com", "Secure1!", true);

        _userContextMock
            .Setup(u => u.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), null, null, null, null, null, null, null, null, null, null))
            .ReturnsAsync((AuthenticatedUser?)null);

        // Act
        var result = await _sut.HandleRegisterGeneralUserAsync(command);

        // Assert
        result.Should().BeNull();
    }

    // ─── HandleRegisterGeneralUserAsync — Escenarios de error ────────────

    [Fact]
    public async Task HandleRegisterGeneralUserAsync_PrivacyPolicyNotAccepted_ReturnsNull()
    {
        // Arrange — AcceptsPrivacyPolicy = false invalida el command
        var command = new RegisterGeneralUserCommand("Ana", "García", "ana@test.com", "Secure1!",
            acceptsPrivacyPolicy: false);

        // Act
        var result = await _sut.HandleRegisterGeneralUserAsync(command);

        // Assert
        result.Should().BeNull();
        _userContextMock.Verify(u => u.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            null, null, null, null, null, null, null, null, null, null), Times.Never);
    }

    [Fact]
    public async Task HandleRegisterGeneralUserAsync_ShortPassword_ReturnsNull()
    {
        // Arrange — contraseña de 4 caracteres no pasa IsValid()
        var command = new RegisterGeneralUserCommand("Ana", "García", "ana@test.com", "abc", true);

        // Act
        var result = await _sut.HandleRegisterGeneralUserAsync(command);

        // Assert
        result.Should().BeNull();
    }

    // ─── HandleSendPasswordResetAsync — Escenarios felices ───────────────

    [Fact]
    public async Task HandleSendPasswordResetAsync_ExistingEmail_SendsEmailAndReturnsTrue()
    {
        // Arrange
        var user    = BuildGeneralUser();
        var command = new SendPasswordResetCommand("ana@test.com");

        _userContextMock
            .Setup(u => u.GetUserByEmailAsync("ana@test.com"))
            .ReturnsAsync(user);

        _userContextMock
            .Setup(u => u.SendPasswordResetEmailAsync(user, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleSendPasswordResetAsync(command);

        // Assert
        result.Should().BeTrue();
        _userContextMock.Verify(u => u.SendPasswordResetEmailAsync(user, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleSendPasswordResetAsync_NonExistentEmail_ReturnsTrueWithoutSending()
    {
        // Arrange — devuelve true igual para no revelar si el email existe
        var command = new SendPasswordResetCommand("noexiste@test.com");

        _userContextMock
            .Setup(u => u.GetUserByEmailAsync("noexiste@test.com"))
            .ReturnsAsync((AuthenticatedUser?)null);

        // Act
        var result = await _sut.HandleSendPasswordResetAsync(command);

        // Assert
        result.Should().BeTrue();
        _userContextMock.Verify(u => u.SendPasswordResetEmailAsync(It.IsAny<AuthenticatedUser>(), It.IsAny<string>()), Times.Never);
    }

    // ─── HandleSendPasswordResetAsync — Escenarios de error ──────────────

    [Fact]
    public async Task HandleSendPasswordResetAsync_InvalidEmail_ReturnsFalse()
    {
        // Arrange
        var command = new SendPasswordResetCommand("not-an-email");

        // Act
        var result = await _sut.HandleSendPasswordResetAsync(command);

        // Assert — IsValid() falla, short-circuits a false
        result.Should().BeFalse();
    }

    // ─── HandleResetPasswordAsync — Escenarios felices ───────────────────

    [Fact]
    public async Task HandleResetPasswordAsync_InvalidCommand_ReturnsFalse()
    {
        // Arrange — contraseña demasiado corta
        var command = new ResetPasswordCommand("some-token", "ana@test.com", "abc");

        // Act
        var result = await _sut.HandleResetPasswordAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleResetPasswordAsync_PasswordNotComplex_ReturnsFalse()
    {
        // Arrange — cumple longitud mínima pero no complejidad
        var command = new ResetPasswordCommand("some-token", "ana@test.com", "simplepassword");

        // Act
        var result = await _sut.HandleResetPasswordAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleResetPasswordAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange — token inválido, TokenService.ValidatePasswordResetToken devuelve false
        var command = new ResetPasswordCommand("invalid-token", "ana@test.com", "NewPass1!");

        // Act
        var result = await _sut.HandleResetPasswordAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    // ─── HandleRegisterPsychologistAsync — Escenarios felices ────────────

    [Fact]
    public async Task HandleRegisterPsychologistAsync_ValidCommand_ReturnsUserId()
    {
        // Arrange
        var user    = BuildPsychologistUser();
        var command = new RegisterPsychologistCommand(
            firstName:           "Carlos",
            lastName:            "López",
            email:               "doctor@test.com",
            password:            "Secure1!",
            professionalLicense: "PSY-123",
            yearsOfExperience:   8,
            collegiateRegion:    "Lima",
            specialties:         new[] { "Cognitive Behavioral Therapy" },
            university:          "UNMSM",
            graduationYear:      2015,
            acceptsPrivacyPolicy: true);

        _userContextMock
            .Setup(u => u.CreateUserAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Psychologist",
                It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
                null, null, null, null))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleRegisterPsychologistAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("psy-001");
    }

    [Fact]
    public async Task HandleRegisterPsychologistAsync_InvalidCommand_ReturnsNull()
    {
        // Arrange — contraseña vacía invalida el command
        var command = new RegisterPsychologistCommand(
            firstName: "Carlos", lastName: "López", email: "doctor@test.com",
            password: "", professionalLicense: "PSY-123",
            yearsOfExperience: 8, collegiateRegion: "Lima",
            specialties: new[] { "CBT" }, university: "UNMSM",
            graduationYear: 2015, acceptsPrivacyPolicy: true);

        // Act
        var result = await _sut.HandleRegisterPsychologistAsync(command);

        // Assert
        result.Should().BeNull();
        _userContextMock.Verify(u => u.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
            null, null, null, null), Times.Never);
    }
}
