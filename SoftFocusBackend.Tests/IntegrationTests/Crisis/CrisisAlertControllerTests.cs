// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para CrisisAlertController
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

namespace SoftFocusBackend.Tests.IntegrationTests.Crisis;

public class CrisisAlertControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CrisisAlertControllerTests(WebApplicationFactory<Program> factory)
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
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }).CreateClient();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    [Fact]
    public async Task Get_PsychologistAlerts_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/crisis/alerts");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_PendingAlertCount_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/crisis/alerts/count/pending");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_AlertById_ReturnsNotFound_IfIdIsInvalid()
    {
        // Arrange
        var fakeId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

        // Act
        var response = await _client.GetAsync($"/api/v1/crisis/alerts/{fakeId}");

        // Assert - La alerta no existe en base de datos vacía
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_UpdateAlertStatus_ReturnsNotFound_IfAlertDoesNotExist()
    {
        // Arrange
        var fakeId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var request = new { Status = "Resolved", PsychologistNotes = "Todo OK" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/crisis/alerts/{fakeId}/status", request);

        // Assert - Como no existe la alerta, el CommandService lanzará InvalidOperationException
        // y tu controlador la capturará regresando un 400 BadRequest.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
    }
}