using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.AI.Application.Internal.CommandServices;
using SoftFocusBackend.AI.Application.Internal.QueryServices;
using SoftFocusBackend.AI.Domain.Model.Queries;
using SoftFocusBackend.AI.Interfaces.REST.Resources;
using SoftFocusBackend.AI.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.AI.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/ai/emotion")]
[Authorize]
[Produces("application/json")]
public class AIEmotionController : ControllerBase
{
    private readonly AIEmotionCommandService _emotionCommandService;
    private readonly AIUsageQueryService _usageQueryService;
    private readonly ILogger<AIEmotionController> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public AIEmotionController(
        AIEmotionCommandService emotionCommandService,
        AIUsageQueryService usageQueryService,
        ILogger<AIEmotionController> logger)
    {
        _emotionCommandService = emotionCommandService ?? throw new ArgumentNullException(nameof(emotionCommandService));
        _usageQueryService = usageQueryService ?? throw new ArgumentNullException(nameof(usageQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Analyze facial emotion",
        Description = "Analyzes a user's facial expression from an uploaded image using AI. Returns detected emotion with confidence score. Optionally creates a check-in entry. Subject to usage limits.",
        OperationId = "AnalyzeFacialEmotion",
        Tags = new[] { "AI Emotion" }
    )]
    [ProducesResponseType(typeof(EmotionAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeEmotion(IFormFile image, [FromForm] bool autoCheckIn = true)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized emotion analysis attempt");
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            if (image == null || image.Length == 0)
            {
                _logger.LogWarning("No image provided for emotion analysis from user {UserId}", userId);
                return BadRequest(AIResourceAssembler.ToErrorResponse("Image is required"));
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid image format {Extension} from user {UserId}", extension, userId);
                return BadRequest(AIResourceAssembler.ToErrorResponse("Invalid image format. Only JPG and PNG are allowed"));
            }

            if (image.Length > MaxFileSize)
            {
                _logger.LogWarning("Image too large ({Size} bytes) from user {UserId}", image.Length, userId);
                return StatusCode(StatusCodes.Status413PayloadTooLarge,
                    AIResourceAssembler.ToErrorResponse("Image size exceeds 5MB limit"));
            }

            _logger.LogInformation("Processing emotion analysis for user {UserId} with autoCheckIn={AutoCheckIn}", userId, autoCheckIn);

            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            var command = AIResourceAssembler.ToCommand(imageBytes, userId, autoCheckIn);

            var result = await _emotionCommandService.HandleAnalyzeEmotionAsync(command);

            if (result.IsLimitExceeded)
            {
                _logger.LogWarning("User {UserId} exceeded facial analysis usage limit", userId);
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    AIResourceAssembler.ToErrorResponse("Facial analysis usage limit exceeded. Please upgrade to Premium or wait for weekly reset.", result.Usage));
            }

            if (result.Analysis == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AIResourceAssembler.ToErrorResponse("Failed to analyze emotion"));
            }

            var response = AIResourceAssembler.ToEmotionResponse(result.Analysis);

            _logger.LogInformation("Emotion analyzed successfully for user {UserId}: {Emotion} ({Confidence:P0})",
                userId, result.Analysis.DetectedEmotion, result.Analysis.Confidence);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error analyzing emotion");
            return BadRequest(AIResourceAssembler.ToErrorResponse("No face detected in the image or invalid image format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing emotion analysis");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while analyzing the image"));
        }
    }

    [HttpGet("usage")]
    [SwaggerOperation(
        Summary = "Get AI usage statistics",
        Description = "Retrieves the current user's AI feature usage statistics including facial analysis and chat limits.",
        OperationId = "GetAIEmotionUsage",
        Tags = new[] { "AI Emotion" }
    )]
    [ProducesResponseType(typeof(AIUsageStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsage()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            var query = new GetAIUsageStatsQuery(userId);
            var usage = await _usageQueryService.HandleGetUsageStatsAsync(query);

            var response = AIResourceAssembler.ToUsageStatsResponse(usage);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while getting usage stats"));
        }
    }

    private string? GetUserId()
    {
        return User.FindFirst("user_id")?.Value;
    }
}
