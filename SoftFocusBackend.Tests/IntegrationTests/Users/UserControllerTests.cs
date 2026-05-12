// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para UserController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Users;

[Collection("Integration")]
public class UserControllerTests
{
    private readonly HttpClient _client;

    public UserControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── GET /api/v1/users/profile ────────────────────────────────────────

    [Fact]
    public async Task Get_Profile_Authenticated_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/profile");

        // Assert — 200 si el usuario test existe en DB, 404 si la DB de test está vacía
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    // ─── PUT /api/v1/users/profile ────────────────────────────────────────

    [Fact]
    public async Task Put_Profile_WithEmptyBody_ReturnsBadRequestOrNotFound()
    {
        // Arrange — multipart/form-data vacío
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PutAsync("/api/v1/users/profile", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    // ─── DELETE /api/v1/users/profile ────────────────────────────────────

    [Fact]
    public async Task Delete_Profile_Authenticated_ReturnsBadRequestOrOk()
    {
        // Act — el usuario test-user-id-123 probablemente no existe en DB de test
        var response = await _client.DeleteAsync("/api/v1/users/profile");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    // ─── GET /api/v1/users/psychologists/directory ───────────────────────

    [Fact]
    public async Task Get_PsychologistsDirectory_ReturnsOk()
    {
        // Act — endpoint público
        var response = await _client.GetAsync("/api/v1/users/psychologists/directory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_PsychologistsDirectory_WithPagination_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/psychologists/directory?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_PsychologistsDirectory_WithSearchTerm_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/psychologists/directory?searchTerm=ana");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── GET /api/v1/users/psychologists/{id} ────────────────────────────

    [Fact]
    public async Task Get_PsychologistById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/psychologists/id-inexistente-123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_PsychologistById_WithSpaceId_ReturnsErrorStatus()
    {
        // Act — id con espacio codificado
        var response = await _client.GetAsync("/api/v1/users/psychologists/%20");

        // Assert — el router puede responder con varios códigos de error
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}
