using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Infrastructure.Services;

public class InvitationCodeService : IInvitationCodeService
{
    private readonly IPsychologistRepository _psychologistRepository;
    private readonly ILogger<InvitationCodeService> _logger;

    public InvitationCodeService(IPsychologistRepository psychologistRepository, ILogger<InvitationCodeService> logger)
    {
        _psychologistRepository = psychologistRepository ?? throw new ArgumentNullException(nameof(psychologistRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InvitationCode> GenerateUniqueCodeAsync()
    {
        const int maxAttempts = 10;
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            var code = InvitationCode.Generate();
            
            if (await IsCodeUniqueAsync(code.Value))
            {
                _logger.LogDebug("Generated unique invitation code: {Code}", code.Value);
                return code;
            }

            attempts++;
        }

        throw new InvalidOperationException("Unable to generate unique invitation code after maximum attempts");
    }

    public async Task<bool> IsCodeUniqueAsync(string code)
    {
        try
        {
            var existingPsychologist = await _psychologistRepository.FindByInvitationCodeAsync(code);
            return existingPsychologist == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking code uniqueness: {Code}", code);
            return false;
        }
    }

    public async Task<bool> ShouldRegenerateCodeAsync(InvitationCode currentCode)
    {
        await Task.CompletedTask;
        
        return currentCode.IsExpired() || 
               currentCode.TimeUntilExpiration() < TimeSpan.FromHours(2);
    }

    public async Task<string?> FindPsychologistByCodeAsync(string invitationCode)
    {
        try
        {
            var psychologist = await _psychologistRepository.FindByInvitationCodeAsync(invitationCode);
            return psychologist?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding psychologist by code: {Code}", invitationCode);
            return null;
        }
    }

    public async Task RegenerateExpiredCodesAsync()
    {
        try
        {
            _logger.LogInformation("Starting automatic regeneration of expired invitation codes");
            
            await _psychologistRepository.RegenerateExpiredCodesAsync();
            
            _logger.LogInformation("Completed automatic regeneration of expired invitation codes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic code regeneration");
            throw;
        }
    }

    public async Task<int> GetActiveCodesCountAsync()
    {
        try
        {
            var verifiedPsychologists = await _psychologistRepository.FindVerifiedPsychologistsAsync();
            var activeCodesCount = verifiedPsychologists.Count(p => !p.IsInvitationCodeExpired());
            
            _logger.LogDebug("Active invitation codes count: {Count}", activeCodesCount);
            return activeCodesCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active codes count");
            return 0;
        }
    }
}