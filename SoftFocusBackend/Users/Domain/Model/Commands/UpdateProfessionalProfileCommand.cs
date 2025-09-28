namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record UpdateProfessionalProfileCommand
{
    public string UserId { get; init; }
    public string? ProfessionalBio { get; init; }
    public bool? IsAcceptingNewPatients { get; init; }
    public int? MaxPatientsCapacity { get; init; }
    public List<string>? TargetAudience { get; init; }
    public List<string>? Languages { get; init; }
    public string? BusinessName { get; init; }
    public string? BusinessAddress { get; init; }
    public string? BankAccount { get; init; }
    public string? PaymentMethods { get; init; }
    public bool? IsProfileVisibleInDirectory { get; init; }
    public bool? AllowsDirectMessages { get; init; }
    public DateTime RequestedAt { get; init; }

    public UpdateProfessionalProfileCommand(string userId, string? professionalBio = null,
        bool? isAcceptingNewPatients = null, int? maxPatientsCapacity = null,
        List<string>? targetAudience = null, List<string>? languages = null,
        string? businessName = null, string? businessAddress = null,
        string? bankAccount = null, string? paymentMethods = null,
        bool? isProfileVisibleInDirectory = null, bool? allowsDirectMessages = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ProfessionalBio = professionalBio?.Trim();
        IsAcceptingNewPatients = isAcceptingNewPatients;
        MaxPatientsCapacity = maxPatientsCapacity;
        TargetAudience = targetAudience;
        Languages = languages;
        BusinessName = businessName?.Trim();
        BusinessAddress = businessAddress?.Trim();
        BankAccount = bankAccount?.Trim();
        PaymentMethods = paymentMethods?.Trim();
        IsProfileVisibleInDirectory = isProfileVisibleInDirectory;
        AllowsDirectMessages = allowsDirectMessages;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               (MaxPatientsCapacity == null || MaxPatientsCapacity > 0);
    }

    public string GetAuditString()
    {
        return $"UserId: {UserId} | RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC";
    }
}