// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para AuthController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Auth;

[Collection("Integration")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        // Sin header de autenticación — los endpoints de auth son [AllowAnonymous]
    }

    // ─── POST /api/v1/auth/login ──────────────────────────────────────────

    [Fact]
    public async Task Post_Login_WithInvalidBody_ReturnsBadRequest()
    {
        // Arrange — body sin los campos requeridos
        var body = new { };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_Login_WithWrongCredentials_ReturnsUnauthorized()
    {
        // Arrange — credenciales con formato válido pero usuario inexistente en DB de test
        var body = new { email = "noexiste@test.com", password = "WrongPass1!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Login_WithMalformedEmail_ReturnsBadRequestOrUnauthorized()
    {
        // Arrange
        var body = new { email = "not-an-email", password = "SomePass1!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    // ─── POST /api/v1/auth/register/general ──────────────────────────────

    [Fact]
    public async Task Post_RegisterGeneral_WithInvalidBody_ReturnsBadRequest()
    {
        // Arrange — faltan campos obligatorios
        var body = new { email = "nopass@test.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/general", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_RegisterGeneral_WithPrivacyPolicyFalse_ReturnsBadRequest()
    {
        // Arrange — acceptsPrivacyPolicy = false invalida el command
        var body = new
        {
            firstName = "Ana",
            lastName = "García",
            email = "ana@test.com",
            password = "Secure1!",
            acceptsPrivacyPolicy = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/general", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_RegisterGeneral_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var body = new
        {
            firstName = "Ana",
            lastName = "García",
            email = "ana@test.com",
            password = "abc",
            acceptsPrivacyPolicy = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/general", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── POST /api/v1/auth/forgot-password ───────────────────────────────

    [Fact]
    public async Task Post_ForgotPassword_WithValidEmail_ReturnsOk()
    {
        // Arrange — siempre devuelve 200 para no revelar si el email existe
        var body = new { email = "cualquier@correo.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_ForgotPassword_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange — email vacío
        var body = new { email = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    // ─── POST /api/v1/auth/reset-password ────────────────────────────────

    [Fact]
    public async Task Post_ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange — token inválido no pasará la validación del TokenService
        var body = new
        {
            token = "token-invalido",
            email = "ana@test.com",
            newPassword = "NewPass1!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_ResetPassword_WithMissingFields_ReturnsBadRequest()
    {
        // Arrange
        var body = new { email = "ana@test.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_ResetPassword_WithNonComplexPassword_ReturnsBadRequest()
    {
        // Arrange — contraseña sin mayúscula/número/especial
        var body = new
        {
            token = "algún-token",
            email = "ana@test.com",
            newPassword = "simplepassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── POST /api/v1/auth/oauth/verify (Google/Facebook) ────────────────

    [Fact]
    public async Task Post_OAuthVerify_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange — token de Google falso; el servicio OAuth fallará al validarlo
        var body = new
        {
            provider = "google",
            accessToken = "token-google-invalido-fake"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/oauth/verify", body);

        // Assert — el servicio no puede verificar el token con Google, devuelve 401
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_OAuthVerify_WithMissingProvider_ReturnsBadRequest()
    {
        // Arrange — falta el campo provider
        var body = new { accessToken = "algún-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/oauth/verify", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_OAuthVerify_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var body = new { };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/oauth/verify", body);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
