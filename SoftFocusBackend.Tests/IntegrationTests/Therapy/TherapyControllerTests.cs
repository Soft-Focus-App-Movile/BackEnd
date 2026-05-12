// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para TherapyController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tests.IntegrationTests.Therapy;

public class TherapyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TherapyControllerTests(WebApplicationFactory<Program> factory)
    {
        // 1. Variables de Entorno para TokenSettings
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", "EstaEsUnaClaveSecretaFalsaParaPruebasMuyLarga123456!");
        Environment.SetEnvironmentVariable("TokenSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("TokenSettings__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("TokenSettings__ExpirationHours", "1");
        Environment.SetEnvironmentVariable("TokenSettings__ClockSkewMinutes", "5");

        // 2. Variables de Entorno para MongoDbSettings
        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", "mongodb://localhost:27017");
        Environment.SetEnvironmentVariable("MongoDbSettings__DatabaseName", "softfocus_test_db");

        // 3. Variables de Entorno para CloudinarySettings
        Environment.SetEnvironmentVariable("CloudinarySettings__CloudName", "test_cloud_name");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiKey", "123456789012345");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiSecret", "test_api_secret_fake_12345");

        _client = factory.WithWebHostBuilder(builder =>
        {
            // Mantenemos nuestro mock de autenticación
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }).CreateClient();

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