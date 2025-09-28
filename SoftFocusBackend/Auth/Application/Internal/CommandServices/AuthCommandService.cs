using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Auth.Domain.Services;
using SoftFocusBackend.Auth.Infrastructure.OAuth.Services;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Services;

namespace SoftFocusBackend.Auth.Application.Internal.CommandServices;

public class AuthCommandService : IAuthCommandService
{
    private readonly IUserContextService _userContextService;
    private readonly TokenService _tokenService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthCommandService> _logger;

    public AuthCommandService(
        IUserContextService userContextService,
        TokenService tokenService,
        IServiceProvider serviceProvider,
        ILogger<AuthCommandService> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthToken?> HandleSignInAsync(SignInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing sign-in command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid sign-in command for email: {Email}", command.Email);
                return null;
            }

            var authenticatedUser = await _userContextService.AuthenticateUserAsync(
                command.Email, 
                command.Password);

            if (authenticatedUser == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}", command.Email);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {UserId} - {Email}", 
                authenticatedUser.Id, authenticatedUser.Email);

            var authToken = _tokenService.GenerateToken(authenticatedUser);

            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                try
                {
                    await scopedUserService.UpdateUserLastLoginAsync(authenticatedUser.Id);
                    _logger.LogDebug("Last login updated for user: {UserId}", authenticatedUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for user: {UserId}", authenticatedUser.Id);
                }
            });

            _logger.LogInformation("Sign-in completed successfully for user: {UserId}", authenticatedUser.Id);
            return authToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sign-in command for email: {Email}", command.Email);
            return null;
        }
    }

    public async Task<AuthToken?> HandleOAuthSignInAsync(OAuthSignInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing OAuth sign-in command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid OAuth sign-in command for provider: {Provider}", command.Provider.Name);
                return null;
            }

            var userInfo = await GetUserInfoFromOAuthProvider(command.Provider);
            if (userInfo == null)
            {
                _logger.LogWarning("Failed to get user info from OAuth provider: {Provider}", command.Provider.Name);
                return null;
            }

            var authenticatedUser = await _userContextService.CreateOrGetOAuthUserAsync(
                userInfo.Value.Email, 
                userInfo.Value.FullName, 
                command.Provider.Name);

            if (authenticatedUser == null)
            {
                _logger.LogWarning("Failed to create/get OAuth user for email: {Email}", userInfo.Value.Email); 
                return null;
            }

            _logger.LogInformation("OAuth user authenticated successfully: {UserId} - {Email}", 
                authenticatedUser.Id, authenticatedUser.Email);

            var authToken = _tokenService.GenerateToken(authenticatedUser);

            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                try
                {
                    await scopedUserService.UpdateUserLastLoginAsync(authenticatedUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for OAuth user: {UserId}", authenticatedUser.Id);
                }
            });

            return authToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OAuth sign-in for provider: {Provider}", command.Provider.Name);
            return null;
        }
    }

    public async Task<bool> HandleSendPasswordResetAsync(SendPasswordResetCommand command)
    {
        try
        {
            _logger.LogInformation("Processing send password reset command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid send password reset command for email: {Email}", command.Email);
                return false;
            }

            var user = await _userContextService.GetUserByEmailAsync(command.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", command.Email);
                return true;
            }

            _logger.LogInformation("User found for password reset: {UserId} - {Email}", user.Id, user.Email);

            var resetToken = _tokenService.GeneratePasswordResetToken(user.Id, user.Email);

            await _userContextService.SendPasswordResetEmailAsync(user, resetToken);

            _logger.LogInformation("Password reset email sent successfully for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing send password reset command for email: {Email}", command.Email);
            return true;
        }
    }

    public async Task<bool> HandleResetPasswordAsync(ResetPasswordCommand command)
    {
        try
        {
            _logger.LogInformation("Processing reset password command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid reset password command for email: {Email}", command.Email);
                return false;
            }

            if (!command.IsPasswordComplex())
            {
                _logger.LogWarning("Password does not meet complexity requirements for email: {Email}", command.Email);
                return false;
            }

            var (isValid, userId, tokenEmail) = _tokenService.ValidatePasswordResetToken(command.Token);
            if (!isValid)
            {
                _logger.LogWarning("Invalid or expired reset token for email: {Email}", command.Email);
                return false;
            }

            if (!string.Equals(command.Email, tokenEmail, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email mismatch in reset password: requested={RequestedEmail}, token={TokenEmail}", 
                    command.Email, tokenEmail);
                return false;
            }

            var user = await _userContextService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for password reset: {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Resetting password for user: {UserId} - {Email}", user.Id, user.Email);

            var success = await _userContextService.ResetUserPasswordAsync(user.Id, command.NewPassword);
            if (!success)
            {
                _logger.LogError("Failed to reset password for user: {UserId}", user.Id);
                return false;
            }

            _logger.LogInformation("Password reset completed successfully for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reset password command for email: {Email}", command.Email);
            return false;
        }
    }

    private async Task<(string Email, string FullName)?> GetUserInfoFromOAuthProvider(OAuthProvider provider)
    {
        try
        {
            switch (provider.Name.ToLower())
            {
                case "google":
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var googleService = scope.ServiceProvider.GetRequiredService<GoogleOAuthService>();
                        var userInfo = await googleService.GetUserInfoAsync(provider.AccessToken);
                        return userInfo != null ? (userInfo.Value.Email, userInfo.Value.FullName) : null;
                    }
            
                case "facebook":
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var facebookService = scope.ServiceProvider.GetRequiredService<FacebookOAuthService>();
                        var userInfo = await facebookService.GetUserInfoAsync(provider.AccessToken);
                        return userInfo != null ? (userInfo.Value.Email, userInfo.Value.FullName) : null;
                    }
            
                default:
                    _logger.LogWarning("Unsupported OAuth provider: {Provider}", provider.Name);
                    return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info from OAuth provider: {Provider}", provider.Name);
            return null;
        }
    }
}