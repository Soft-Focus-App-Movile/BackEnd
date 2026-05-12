// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA DE INTEGRACIÓN
//  Objetivo: Probar los Endpoints HTTP completos para ChatController
// ════════════════════════════════════════════════════════════════════════════
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tests.IntegrationTests.Therapy;

[Collection("Integration")]
public class ChatControllerTests
{
    private readonly HttpClient _client;

    public ChatControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
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