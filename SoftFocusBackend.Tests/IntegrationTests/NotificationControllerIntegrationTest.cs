using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SoftFocusBackend.Tests.IntegrationTests;

public class NotificationControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public NotificationControllerIntegrationTest(WebApplicationFactory<Program> factory)
    {
        
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", "EstaEsUnaClaveSuperSecretaDeAlMenos32Caracteres!");
        Environment.SetEnvironmentVariable("TokenSettings__Issuer", "SoftFocusBackend");
        Environment.SetEnvironmentVariable("TokenSettings__Audience", "SoftFocusResources");
        Environment.SetEnvironmentVariable("TokenSettings__ClockSkewMinutes", "5");
        Environment.SetEnvironmentVariable("TokenSettings__ValidateIssuerSigningKey", "true");
        Environment.SetEnvironmentVariable("TokenSettings__ValidateIssuer", "true");
        Environment.SetEnvironmentVariable("TokenSettings__ValidateAudience", "true");
        Environment.SetEnvironmentVariable("TokenSettings__ValidateLifetime", "true");

        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", "mongodb://localhost:27017/?connectTimeoutMS=1&serverSelectionTimeoutMS=1");
        Environment.SetEnvironmentVariable("MongoDbSettings__DatabaseName", "SoftFocusTestDB");

        Environment.SetEnvironmentVariable("GoogleOAuthSettings__ClientId", "test-id");
        Environment.SetEnvironmentVariable("GoogleOAuthSettings__ClientSecret", "test-secret");

        Environment.SetEnvironmentVariable("CloudinarySettings__CloudName", "test-cloud");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiKey", "123456789012345");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiSecret", "EstaEsUnaClaveSecretaFalsa123");

        _factory = factory;
    }

    [Fact]
    public async Task GetNotifications_WhenCalledWithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        // Creamos el cliente, pero NO le ponemos ningún Token de autorización
        var client = _factory.CreateClient();

        // Act
        // Intentamos entrar a ver la lista de notificaciones como si fuéramos un intruso
        var response = await client.GetAsync("/api/v1/notifications");

        // Assert
        // Verificamos que el servidor nos rebote con un error 401 Unauthorized (No autorizado)
        // Esto demuestra que tu [Authorize] funciona a la perfección.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}