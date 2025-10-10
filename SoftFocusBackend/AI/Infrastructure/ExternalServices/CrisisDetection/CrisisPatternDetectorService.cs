using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.CrisisDetection;

public class CrisisPatternDetectorService : ICrisisPatternDetector
{
    private readonly IEmotionAnalysisRepository _emotionAnalysisRepository;
    private readonly ILogger<CrisisPatternDetectorService> _logger;

    private static readonly string[] CriticalKeywords = new[]
    {
        "suicidio", "suicidarme", "matarme", "hacerme daño",
        "terminar con todo", "acabar con mi vida", "no vale la pena vivir",
        "mejor muerto", "quiero morir", "desaparecer para siempre"
    };

    private static readonly string[] NegativeEmotions = new[] { "sadness", "fear", "anger" };

    public CrisisPatternDetectorService(
        IEmotionAnalysisRepository emotionAnalysisRepository,
        ILogger<CrisisPatternDetectorService> logger)
    {
        _emotionAnalysisRepository = emotionAnalysisRepository;
        _logger = logger;
    }

    public Task<CrisisPattern?> DetectFromChatAsync(ChatMessage message)
    {
        var lowerContent = message.Content.ToLower();

        foreach (var keyword in CriticalKeywords)
        {
            if (lowerContent.Contains(keyword))
            {
                _logger.LogWarning("Critical keyword detected in chat message: {Keyword}", keyword);
                return Task.FromResult<CrisisPattern?>(new CrisisPattern(
                    CrisisSeverity.Critical,
                    $"Critical keyword detected: '{keyword}'"
                ));
            }
        }

        return Task.FromResult<CrisisPattern?>(null);
    }

    public async Task<CrisisPattern?> DetectFromEmotionPatternAsync(string userId, EmotionAnalysis newAnalysis)
    {
        try
        {
            var last7Days = await _emotionAnalysisRepository.GetLast7DaysAsync(userId);

            var allAnalyses = last7Days.Append(newAnalysis)
                .OrderBy(a => a.AnalyzedAt)
                .ToList();

            if (allAnalyses.Count < 3)
            {
                return null;
            }

            int consecutiveNegativeDays = 0;
            int maxConsecutive = 0;

            foreach (var analysis in allAnalyses)
            {
                if (NegativeEmotions.Contains(analysis.DetectedEmotion.ToLower()) && analysis.Confidence > 0.85)
                {
                    consecutiveNegativeDays++;
                    maxConsecutive = Math.Max(maxConsecutive, consecutiveNegativeDays);
                }
                else
                {
                    consecutiveNegativeDays = 0;
                }
            }

            if (maxConsecutive >= 5)
            {
                _logger.LogWarning("High severity crisis pattern detected for user {UserId}: {Days} consecutive negative days",
                    userId, maxConsecutive);
                return new CrisisPattern(
                    CrisisSeverity.High,
                    $"{maxConsecutive} consecutive days of negative emotions with high confidence"
                );
            }

            if (maxConsecutive >= 3)
            {
                _logger.LogWarning("Moderate severity crisis pattern detected for user {UserId}: {Days} consecutive negative days",
                    userId, maxConsecutive);
                return new CrisisPattern(
                    CrisisSeverity.Moderate,
                    $"{maxConsecutive} consecutive days of negative emotions with high confidence"
                );
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting crisis pattern from emotions for user {UserId}", userId);
            return null;
        }
    }
}
