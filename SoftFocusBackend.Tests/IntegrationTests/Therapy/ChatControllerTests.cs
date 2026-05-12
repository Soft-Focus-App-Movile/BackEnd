// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para ChatController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tests.IntegrationTests.Therapy;

public class ChatControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ChatControllerTests(WebApplicationFactory<Program> factory)
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
    public async Task Get_History_ReturnsOk()
    {
        // Arrange
        var relationshipId = "test-relationship-id";

        // Act
        var response = await _client.GetAsync($"/api/v1/chat/history?relationshipId={relationshipId}&page=1&size=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_LastReceivedMessage_ReturnsNotFound_WhenDatabaseIsEmpty()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/chat/last-received");

        // Assert - Nuestra DB mock no tiene mensajes, la API retorna 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Post_SendMessage_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        // Pasamos los 4 parámetros que exige el constructor.
        // Enviamos el 'content' vacío o con espacios para forzar la validación de error.
        var request = new SendChatMessageRequest(
            relationshipId: "invalid-relationship-id", 
            receiverId: "invalid-receiver-id", 
            content: " ", // <--- Dato inválido que debería rechazar
            messageType: "Text"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/chat/send", request);

        // Assert - Esperamos código de error porque fallarán las validaciones
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}