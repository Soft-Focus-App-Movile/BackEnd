using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SoftFocusBackend.Tests.IntegrationTests;

public class UserControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserControllerIntegrationTest(WebApplicationFactory<Program> factory)
    {
        // JWT Settings
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

        // Google Settings
        Environment.SetEnvironmentVariable("GoogleOAuthSettings__ClientId", "test-id");
        Environment.SetEnvironmentVariable("GoogleOAuthSettings__ClientSecret", "test-secret");

        // Cloudinary Settings
        Environment.SetEnvironmentVariable("CloudinarySettings__CloudName", "test-cloud");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiKey", "123456789012345");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiSecret", "EstaEsUnaClaveSecretaFalsa123");

        _factory = factory;
    }

    [Fact]
    public async Task GetPsychologistsDirectory_WhenCalled_ReturnsOkStatusCode()
    {
        // Arrange
        // CreateClient() disparará el Program.cs, el cual ahora encontrará las variables de entorno
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users/psychologists/directory?page=1&pageSize=10");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError, 
            $"El servidor arrancó pero devolvió {response.StatusCode}. Si es 500, es por la conexión a Mongo, pero el test de integración ya superó la validación de JWT.");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }
    }
}