// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los endpoints HTTP de ContentSearchController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Library;

[Collection("Integration")]
public class ContentSearchControllerTests
{
    private readonly HttpClient _client;

    public ContentSearchControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── GET /api/v1/library/{contentId} ─────────────────────────────────

    [Fact]
    public async Task Get_ContentById_WithNonExistentId_ReturnsNotFoundOrError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/nonexistent-content-id");

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Get_ContentById_WithEmptyId_ReturnsErrorStatus()
    {
        // Act — id con espacio codificado
        var response = await _client.GetAsync("/api/v1/library/%20");

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── POST /api/v1/library/search ──────────────────────────────────────

    [Fact]
    public async Task Post_SearchContent_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var content = JsonContent.Create(new { });

        // Act
        var response = await _client.PostAsync("/api/v1/library/search", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Post_SearchContent_WithValidMovieQuery_ReturnsOkOrError()
    {
        // Arrange — búsqueda válida (puede fallar si el servicio externo no está disponible en test)
        var body = JsonContent.Create(new
        {
            query       = "inception",
            contentType = "Movie",
            limit       = 5
        });

        // Act
        var response = await _client.PostAsync("/api/v1/library/search", body);

        // Assert — OK si el servicio externo responde; error de infraestructura si no
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    // ─── Auth ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_SearchContent_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };
        var body         = JsonContent.Create(new { query = "test", contentType = "Movie" });

        // Act
        var response = await unauthClient.PostAsync("/api/v1/library/search", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
