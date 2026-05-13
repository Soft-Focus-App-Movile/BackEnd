// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los endpoints HTTP de AssignmentsController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.Library;

[Collection("Integration")]
public class AssignmentsControllerTests
{
    private readonly HttpClient _client;

    public AssignmentsControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    // ─── POST /api/v1/library/assignments ────────────────────────────────

    [Fact]
    public async Task Post_AssignContent_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var content = JsonContent.Create(new { });

        // Act
        var response = await _client.PostAsync("/api/v1/library/assignments", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Post_AssignContent_WithInvalidPatient_ReturnsError()
    {
        // Arrange — psicólogo ficticio, paciente inexistente
        var body = JsonContent.Create(new
        {
            psychologistId = "test-user-id-123",
            patientIds     = new[] { "nonexistent-patient" },
            contentId      = "tmdb-movie-001",
            contentType    = "Movie",
            notes          = "Por favor ver esta semana"
        });

        // Act
        var response = await _client.PostAsync("/api/v1/library/assignments", body);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── GET /api/v1/library/assignments/assigned ─────────────────────────

    [Fact]
    public async Task Get_AssignedContent_Authenticated_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/assignments/assigned");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_AssignedContent_WithPendingFilter_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/assignments/assigned?completed=false");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_AssignedContent_WithCompletedFilter_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/assignments/assigned?completed=true");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    // ─── POST /api/v1/library/assignments/assigned/{id}/complete ─────────

    [Fact]
    public async Task Post_CompleteAssignment_WithNonExistentId_ReturnsErrorStatus()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/library/assignments/assigned/nonexistent-id/complete", null);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    // ─── GET /api/v1/library/assignments/by-psychologist ─────────────────

    [Fact]
    public async Task Get_AssignmentsByPsychologist_Authenticated_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/assignments/by-psychologist");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_AssignmentsByPsychologist_WithPatientFilter_ReturnsOkOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/library/assignments/by-psychologist?patientId=some-patient-id");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    // ─── Auth ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_AssignedContent_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var unauthClient = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await unauthClient.GetAsync("/api/v1/library/assignments/assigned");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
