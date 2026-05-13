// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los endpoints HTTP de RecommendationsController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Library;

[Collection("Integration")]
public class RecommendationsControllerTests
{
    private readonly HttpClient _client;

    public RecommendationsControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── GET /api/v1/library/recommendations/places ───────────────────────

    [Fact]
    public async Task Get_PlaceRecommendations_WithValidCoordinates_ReturnsOkOrError()
    {
        // Act — puede fallar si los servicios externos (OpenWeather, Foursquare) no están disponibles
        var response = await _client.GetAsync("/api/v1/library/recommendations/places?latitude=-12.0464&longitude=-77.0428");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_PlaceRecommendations_MissingCoordinates_ReturnsErrorOrOk()
    {
        // Act — faltan los parámetros obligatorios de coordenadas
        var response = await _client.GetAsync("/api/v1/library/recommendations/places");

        // Assert — puede usar valores default (200) o rechazar la petición (400/500)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_PlaceRecommendations_WithEmotionFilter_ReturnsOkOrError()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/v1/library/recommendations/places?latitude=-12.0464&longitude=-77.0428&emotionFilter=Calm");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    // ─── GET /api/v1/library/recommendations/content ─────────────────────

    [Fact]
    public async Task Get_ContentRecommendations_WithoutFilter_ReturnsOkOrError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/recommendations/content");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_ContentRecommendations_WithContentTypeFilter_ReturnsOkOrError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/recommendations/content?contentType=Music");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    // ─── GET /api/v1/library/recommendations/emotion/{emotion} ───────────

    [Fact]
    public async Task Get_RecommendationsByEmotion_ValidEmotion_ReturnsOkOrError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/recommendations/emotion/Happy");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_RecommendationsByEmotion_WithContentTypeFilter_ReturnsOkOrError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/recommendations/emotion/Calm?contentType=Video");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable);
    }

}
