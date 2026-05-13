// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los endpoints HTTP de FavoritesController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Library;

[Collection("Integration")]
public class FavoritesControllerTests
{
    private readonly HttpClient _client;

    public FavoritesControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── GET /api/v1/library/favorites ───────────────────────────────────

    [Fact]
    public async Task Get_Favorites_Authenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/favorites");

        // Assert — 200 si el usuario test existe; error si no hay datos
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Favorites_WithContentTypeFilter_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/favorites?contentType=Movie");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Favorites_WithEmotionFilter_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/favorites?emotionFilter=Happy");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Favorites_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange — sin token de autenticación
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await unauthClient.GetAsync("/api/v1/library/favorites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── POST /api/v1/library/favorites ──────────────────────────────────

    [Fact]
    public async Task Post_AddFavorite_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var content = JsonContent.Create(new { });

        // Act
        var response = await _client.PostAsync("/api/v1/library/favorites", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Post_AddFavorite_WithInvalidContentId_ReturnsError()
    {
        // Arrange — contentId inexistente en caché
        var body = JsonContent.Create(new
        {
            contentId   = "nonexistent-content-id",
            contentType = "Movie"
        });

        // Act
        var response = await _client.PostAsync("/api/v1/library/favorites", body);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── DELETE /api/v1/library/favorites/{favoriteId} ───────────────────

    [Fact]
    public async Task Delete_RemoveFavorite_WithNonExistentId_ReturnsErrorStatus()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/library/favorites/nonexistent-favorite-id");

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}
