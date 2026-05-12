// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para TherapyController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tests.IntegrationTests.Therapy;

[Collection("Integration")]
public class TherapyControllerTests
{
    private readonly HttpClient _client;

    public TherapyControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    [Fact]
    public async Task Get_PatientDirectory_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/therapy/patients");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_MyRelationship_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/therapy/my-relationship");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Disconnect_ReturnsNotFound_IfRelationshipDoesNotExist()
    {
        // Arrange
        var fakeId = "invalid-relationship-id";

        // Act
        var response = await _client.DeleteAsync($"/api/v1/therapy/disconnect/{fakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}