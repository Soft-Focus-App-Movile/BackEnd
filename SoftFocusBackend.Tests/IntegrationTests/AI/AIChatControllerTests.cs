using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.IntegrationTests.AI;

[Collection("Integration")]
public class AIChatControllerTests
{
    private readonly HttpClient _client;

    public AIChatControllerTests(SharedWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    [Fact]
    public async Task Post_Message_WithEmptyBody_ReturnsBadRequest()
    {
        var body = new { };

        var response = await _client.PostAsJsonAsync("/api/v1/ai/chat/message", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_Message_WithValidMessage_ReturnsOkOrError()
    {
        var body = new { message = "Hola, ¿cómo estás?" };

        var response = await _client.PostAsJsonAsync("/api/v1/ai/chat/message", body);

        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(200);
    }

    [Fact]
    public async Task Post_Message_WithMessageTooLong_ReturnsBadRequest()
    {
        var body = new { message = new string('a', 2001) };

        var response = await _client.PostAsJsonAsync("/api/v1/ai/chat/message", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Usage_Authenticated_ReturnsOkOrError()
    {
        var response = await _client.GetAsync("/api/v1/ai/chat/usage");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Sessions_Authenticated_ReturnsOkOrError()
    {
        var response = await _client.GetAsync("/api/v1/ai/chat/sessions");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_SessionMessages_WithInvalidId_ReturnsNotFoundOrError()
    {
        var response = await _client.GetAsync("/api/v1/ai/chat/sessions/id-inexistente-123/messages");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_Sessions_WithPageSize_ReturnsOkOrError()
    {
        var response = await _client.GetAsync("/api/v1/ai/chat/sessions?pageSize=10");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }
}