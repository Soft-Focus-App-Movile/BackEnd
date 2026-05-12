using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace SoftFocusBackend.Tests.Helpers;

/// <summary>
/// Factory compartida entre todos los integration tests para evitar que múltiples
/// instancias de WebApplicationFactory registren el mismo BsonClassMap dos veces.
/// </summary>
public class SharedWebAppFactory : WebApplicationFactory<Program>
{
    static SharedWebAppFactory()
    {
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", "EstaEsUnaClaveSecretaFalsaParaPruebasMuyLarga123456!");
        Environment.SetEnvironmentVariable("TokenSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("TokenSettings__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("TokenSettings__ExpirationHours", "1");
        Environment.SetEnvironmentVariable("TokenSettings__ClockSkewMinutes", "5");
        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", "mongodb://localhost:27017");
        Environment.SetEnvironmentVariable("MongoDbSettings__DatabaseName", "softfocus_test_db");
        Environment.SetEnvironmentVariable("CloudinarySettings__CloudName", "test_cloud_name");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiKey", "123456789012345");
        Environment.SetEnvironmentVariable("CloudinarySettings__ApiSecret", "test_api_secret_fake_12345");
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }
}

/// <summary>
/// Colección xUnit que comparte una única instancia de SharedWebAppFactory
/// entre todos los integration tests, evitando el registro duplicado de BsonClassMap.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<SharedWebAppFactory> { }
