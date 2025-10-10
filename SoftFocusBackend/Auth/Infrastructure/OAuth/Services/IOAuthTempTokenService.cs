using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Services;

public interface IOAuthTempTokenService
{
    Task<OAuthTempToken> CreateTokenAsync(string email, string fullName, string provider);
    Task<OAuthTempToken?> ValidateAndRetrieveTokenAsync(string token);
    Task RemoveTokenAsync(string token);
}
