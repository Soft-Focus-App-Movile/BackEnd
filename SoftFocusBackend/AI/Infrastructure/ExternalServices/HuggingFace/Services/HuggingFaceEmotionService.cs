using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.AI.Domain.Services;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.HuggingFace.Configuration;

namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.HuggingFace.Services;

public class HuggingFaceEmotionService : IFacialEmotionService
{
    private readonly HuggingFaceSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuggingFaceEmotionService> _logger;

    public HuggingFaceEmotionService(IOptions<HuggingFaceSettings> settings, HttpClient httpClient, ILogger<HuggingFaceEmotionService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        if (!_settings.IsValid())
        {
            _logger.LogWarning("HuggingFaceSettings is not valid. Check configuration.");
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiToken}");
    }

    public async Task<EmotionAnalysisResult> AnalyzeAsync(byte[] imageBytes)
    {
        try
        {
            _logger.LogInformation("Sending image to Hugging Face API for emotion analysis");

            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync(_settings.GetModelUrl(), content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Hugging Face API request failed: {StatusCode}, {Error}", response.StatusCode, errorContent);
                throw new InvalidOperationException($"Hugging Face API failed with status {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Hugging Face API response: {Response}", responseContent);
            return ParseHuggingFaceResponse(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Hugging Face API");
            throw;
        }
    }

    private EmotionAnalysisResult ParseHuggingFaceResponse(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var emotions = new Dictionary<string, double>();

            // Caso 1: Array directo de objetos con label y score
            // Ejemplo: [{"label": "happy", "score": 0.9}, {"label": "sad", "score": 0.1}]
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstElement = root[0];

                // Si cada elemento tiene "label" y "score", es un array directo de emociones
                if (firstElement.TryGetProperty("label", out _) && firstElement.TryGetProperty("score", out _))
                {
                    foreach (var emotion in root.EnumerateArray())
                    {
                        var label = emotion.GetProperty("label").GetString() ?? string.Empty;
                        var score = emotion.GetProperty("score").GetDouble();
                        emotions[label.ToLower()] = score;
                    }
                }
                // Si el primer elemento es un array anidado
                else if (firstElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var emotion in firstElement.EnumerateArray())
                    {
                        var label = emotion.GetProperty("label").GetString() ?? string.Empty;
                        var score = emotion.GetProperty("score").GetDouble();
                        emotions[label.ToLower()] = score;
                    }
                }
                // Si el primer elemento es un objeto con propiedades de emociones
                else if (firstElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in firstElement.EnumerateObject())
                    {
                        emotions[property.Name.ToLower()] = ParseScore(property.Value);
                    }
                }
            }
            // Caso 2: Respuesta es un objeto directo con propiedades de emociones
            else if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    emotions[property.Name.ToLower()] = ParseScore(property.Value);
                }
            }

            if (emotions.Count == 0)
            {
                _logger.LogWarning("No emotions detected in Hugging Face response");
                throw new InvalidOperationException("No emotions detected in response");
            }

            var primaryEmotion = emotions.OrderByDescending(e => e.Value).First();

            return new EmotionAnalysisResult
            {
                PrimaryEmotion = NormalizeEmotionName(primaryEmotion.Key),
                Confidence = primaryEmotion.Value,
                AllEmotions = emotions.ToDictionary(e => NormalizeEmotionName(e.Key), e => e.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Hugging Face response");
            throw;
        }
    }

    private double ParseScore(JsonElement value)
    {
        // Si es un número, devolverlo directamente
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDouble();
        }
        // Si es un string, parsearlo
        else if (value.ValueKind == JsonValueKind.String)
        {
            var stringValue = value.GetString();
            if (double.TryParse(stringValue, out var score))
            {
                return score;
            }
        }

        return 0.0;
    }

    private string NormalizeEmotionName(string emotion)
    {
        return emotion.ToLower() switch
        {
            "joy" or "happy" => "joy",
            "sadness" or "sad" => "sadness",
            "anger" or "angry" => "anger",
            "fear" => "fear",
            "surprise" => "surprise",
            "disgust" => "disgust",
            "neutral" => "neutral",
            _ => emotion.ToLower()
        };
    }
}
