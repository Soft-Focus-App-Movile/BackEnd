using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Services;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Configuration;

namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Services;

public class GeminiChatService : IEmotionalChatService
{
    private readonly GeminiSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiChatService> _logger;

    public GeminiChatService(IOptions<GeminiSettings> settings, HttpClient httpClient, ILogger<GeminiChatService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        if (!_settings.IsValid())
        {
            _logger.LogWarning("GeminiSettings is not valid. Check configuration.");
        }
    }

    public async Task<ChatResponse> SendMessageAsync(GeminiContext context)
    {
        try
        {
            // Detectar crisis en el mensaje del usuario
            var userCrisisDetected = DetectCrisisKeywords(context.CurrentMessage);

            var prompt = BuildPrompt(context);
            var requestBody = BuildRequestBody(prompt);

            _logger.LogInformation("Sending message to Gemini API for user {UserId}", context.UserId);

            var response = await _httpClient.PostAsync(
                _settings.GetGenerateContentUrl(),
                new StringContent(requestBody, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API request failed: {StatusCode}, {Error}", response.StatusCode, errorContent);
                return CreateFallbackResponse();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var chatResponse = ParseGeminiResponse(responseContent);

            // Combinar detección de crisis del usuario y de la respuesta de Gemini
            if (userCrisisDetected && !chatResponse.CrisisDetected)
            {
                chatResponse.CrisisDetected = true;
                chatResponse.CrisisContext = "Crisis keywords detected in user message";
            }
            else if (userCrisisDetected && chatResponse.CrisisDetected)
            {
                chatResponse.CrisisContext = "Crisis keywords detected in both user message and AI response";
            }

            return chatResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API for user {UserId}", context.UserId);
            return CreateFallbackResponse();
        }
    }

    private string BuildPrompt(GeminiContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine(_settings.SystemPrompt);
        sb.AppendLine();
        sb.AppendLine("CONTEXTO DEL USUARIO:");
        sb.AppendLine($"- Patrón emocional: {context.EmotionalPattern}");

        if (context.HasRecentCheckIns())
        {
            sb.AppendLine("- Últimos check-ins emocionales (últimos 7 días):");
            foreach (var checkIn in context.RecentCheckIns)
            {
                sb.AppendLine($"  * {checkIn.Date}: {checkIn.Emotion} (intensidad: {checkIn.Intensity})");
            }
        }

        if (context.HasTherapyGoals())
        {
            sb.AppendLine("- Objetivos terapéuticos actuales:");
            foreach (var goal in context.TherapyGoals)
            {
                sb.AppendLine($"  * {goal}");
            }
        }

        if (context.HasHistory())
        {
            sb.AppendLine();
            sb.AppendLine("HISTORIAL DE CONVERSACIÓN:");
            foreach (var msg in context.ConversationHistory)
            {
                sb.AppendLine($"{msg.Role}: {msg.Content}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("MENSAJE ACTUAL DEL USUARIO:");
        sb.AppendLine(context.CurrentMessage);

        return sb.ToString();
    }

    private string BuildRequestBody(string prompt)
    {
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _settings.Temperature,
                maxOutputTokens = _settings.MaxTokens
            }
        };

        return JsonSerializer.Serialize(request);
    }

    private ChatResponse ParseGeminiResponse(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? string.Empty;

                    var crisisDetected = DetectCrisisKeywords(text);

                    return new ChatResponse
                    {
                        Reply = text,
                        SuggestedQuestions = ExtractSuggestedQuestions(text),
                        RecommendedExercises = ExtractRecommendedExercises(text),
                        CrisisDetected = crisisDetected,
                        CrisisContext = crisisDetected ? "Crisis keywords detected in message" : string.Empty
                    };
                }
            }

            _logger.LogWarning("Unexpected Gemini API response format");
            return CreateFallbackResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            return CreateFallbackResponse();
        }
    }

    private bool DetectCrisisKeywords(string text)
    {
        var crisisKeywords = new[]
        {
            "suicidio", "suicidarme", "matarme", "hacerme daño",
            "terminar con todo", "acabar con mi vida", "no vale la pena vivir",
            "mejor muerto", "quiero morir", "desaparecer para siempre"
        };

        var lowerText = text.ToLower();
        return crisisKeywords.Any(keyword => lowerText.Contains(keyword));
    }

    private List<string> ExtractSuggestedQuestions(string text)
    {
        return new List<string>();
    }

    private List<string> ExtractRecommendedExercises(string text)
    {
        return new List<string>();
    }

    private ChatResponse CreateFallbackResponse()
    {
        return new ChatResponse
        {
            Reply = "Lo siento, estoy teniendo problemas técnicos en este momento. Por favor, intenta nuevamente en unos momentos. Si necesitas ayuda urgente, te recomiendo contactar con un profesional de salud mental.",
            SuggestedQuestions = new List<string>
            {
                "¿Puedo intentar de nuevo más tarde?",
                "¿Cómo puedo contactar con un psicólogo?"
            },
            RecommendedExercises = new List<string>(),
            CrisisDetected = false,
            CrisisContext = string.Empty
        };
    }
}
