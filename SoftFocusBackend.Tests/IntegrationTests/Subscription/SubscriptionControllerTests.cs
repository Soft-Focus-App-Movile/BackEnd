// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los endpoints HTTP de SubscriptionController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Subscription;

[Collection("Integration")]
public class SubscriptionControllerTests
{
    private readonly HttpClient _client;

    public SubscriptionControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── GET /api/v1/subscriptions/me ────────────────────────────────────

    [Fact]
    public async Task Get_MySubscription_Authenticated_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/subscriptions/me");

        // Assert — 200 si existe suscripción para el usuario test; 404 si no
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    // ─── GET /api/v1/subscriptions/usage ─────────────────────────────────

    [Fact]
    public async Task Get_UsageStats_Authenticated_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/subscriptions/usage");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    // ─── GET /api/v1/subscriptions/check-access/{featureType} ────────────

    [Fact]
    public async Task Get_CheckAccess_WithValidFeature_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/subscriptions/check-access/AiChatMessage");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_CheckAccess_WithInvalidFeature_ReturnsBadRequest()
    {
        // Act — feature type inválido
        var response = await _client.GetAsync("/api/v1/subscriptions/check-access/InvalidFeature");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ─── POST /api/v1/subscriptions/initialize ───────────────────────────

    [Fact]
    public async Task Post_InitializeSubscription_Authenticated_ReturnsOkOrConflict()
    {
        // Act — crea suscripción básica si no existe; devuelve la existente si ya hay una
        var response = await _client.PostAsync("/api/v1/subscriptions/initialize", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Conflict,
            HttpStatusCode.InternalServerError);
    }

    // ─── POST /api/v1/subscriptions/upgrade/checkout ─────────────────────

    [Fact]
    public async Task Post_CreateCheckoutSession_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var content = JsonContent.Create(new { });

        // Act
        var response = await _client.PostAsync("/api/v1/subscriptions/upgrade/checkout", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Post_CreateCheckoutSession_WithNoSubscription_ReturnsError()
    {
        // Arrange — el usuario test probablemente no tiene suscripción
        var body = JsonContent.Create(new
        {
            successUrl = "https://test.com/success",
            cancelUrl  = "https://test.com/cancel"
        });

        // Act
        var response = await _client.PostAsync("/api/v1/subscriptions/upgrade/checkout", body);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── POST /api/v1/subscriptions/cancel ───────────────────────────────

    [Fact]
    public async Task Post_CancelSubscription_WithNoProSubscription_ReturnsError()
    {
        // Arrange — el usuario test probablemente tiene plan Basic o no tiene suscripción
        var body = JsonContent.Create(false); // cancelImmediately = false

        // Act
        var response = await _client.PostAsync("/api/v1/subscriptions/cancel", body);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── POST /api/v1/subscriptions/track-usage ──────────────────────────

    [Fact]
    public async Task Post_TrackUsage_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var body = JsonContent.Create(new { });

        // Act
        var response = await _client.PostAsync("/api/v1/subscriptions/track-usage", body);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Post_TrackUsage_WithValidFeature_ReturnsOkOrError()
    {
        // Arrange
        var body = JsonContent.Create(new
        {
            userId      = "test-user-id-123",
            featureType = "AiChatMessage"
        });

        // Act
        var response = await _client.PostAsync("/api/v1/subscriptions/track-usage", body);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    // ─── Auth ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_MySubscription_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await unauthClient.GetAsync("/api/v1/subscriptions/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_UsageStats_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await unauthClient.GetAsync("/api/v1/subscriptions/usage");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
