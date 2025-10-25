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
    private readonly IOAuthTempTokenService _oauthTempTokenService;
    private readonly ILogger<AuthCommandService> _logger;

    public AuthCommandService(
        IUserContextService userContextService,
        TokenService tokenService,
        IServiceProvider serviceProvider,
        IOAuthTempTokenService oauthTempTokenService,
        ILogger<AuthCommandService> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _oauthTempTokenService = oauthTempTokenService ?? throw new ArgumentNullException(nameof(oauthTempTokenService));
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

    public async Task<string?> HandleRegisterGeneralUserAsync(RegisterGeneralUserCommand command)
    {
        try
        {
            _logger.LogInformation("Processing general user registration command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid general user registration command for email: {Email}", command.Email);
                return null;
            }

            var user = await _userContextService.CreateUserAsync(
                command.Email,
                command.Password,
                command.GetFullName(),
                "General");

            if (user == null)
            {
                _logger.LogWarning("Failed to create general user for email: {Email}", command.Email);
                return null;
            }

            _logger.LogInformation("General user registered successfully: {UserId} - {Email}", user.Id, user.Email);
            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing general user registration for email: {Email}", command.Email);
            return null;
        }
    }

    public async Task<string?> HandleRegisterPsychologistAsync(RegisterPsychologistCommand command)
    {
        try
        {
            _logger.LogInformation("Processing psychologist registration command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid psychologist registration command for email: {Email}", command.Email);
                return null;
            }

            var user = await _userContextService.CreateUserAsync(
                command.Email,
                command.Password,
                command.GetFullName(),
                "Psychologist",
                command.ProfessionalLicense,
                command.Specialties);

            if (user == null)
            {
                _logger.LogWarning("Failed to create psychologist user for email: {Email}", command.Email);
                return null;
            }

            _logger.LogInformation("Psychologist registered successfully: {UserId} - {Email}", user.Id, user.Email);
            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing psychologist registration for email: {Email}", command.Email);
            return null;
        }
    }

    public async Task<OAuthVerificationResult?> HandleVerifyOAuthAsync(VerifyOAuthCommand command)
    {
        try
        {
            _logger.LogInformation("Processing OAuth verification command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid OAuth verification command for provider: {Provider}", command.Provider.Name);
                return null;
            }

            var userInfo = await GetUserInfoFromOAuthProvider(command.Provider);
            if (userInfo == null)
            {
                _logger.LogWarning("Failed to get user info from OAuth provider: {Provider}", command.Provider.Name);
                return null;
            }

            // Check if user already exists
            var existingUser = await _userContextService.GetUserByEmailAsync(userInfo.Value.Email);

            if (existingUser != null)
            {
                // User exists - return info to proceed with direct login
                _logger.LogInformation("OAuth user already exists: {Email}, proceeding with login", userInfo.Value.Email);

                var authToken = _tokenService.GenerateToken(existingUser);

                // Update last login asynchronously
                _ = Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                    try
                    {
                        await scopedUserService.UpdateUserLastLoginAsync(existingUser.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update last login for OAuth user: {UserId}", existingUser.Id);
                    }
                });

                return new OAuthVerificationResult
                {
                    Email = existingUser.Email,
                    FullName = existingUser.FullName,
                    Provider = command.Provider.Name,
                    TempToken = authToken.Token, // Return actual JWT token for existing users
                    NeedsRegistration = false,
                    ExistingUserType = existingUser.Role
                };
            }

            // User doesn't exist - create temp token for registration completion
            _logger.LogInformation("OAuth user not found: {Email}, creating temp token for registration", userInfo.Value.Email);

            var tempToken = await _oauthTempTokenService.CreateTokenAsync(
                userInfo.Value.Email,
                userInfo.Value.FullName,
                command.Provider.Name);

            return new OAuthVerificationResult
            {
                Email = userInfo.Value.Email,
                FullName = userInfo.Value.FullName,
                Provider = command.Provider.Name,
                TempToken = tempToken.Token,
                NeedsRegistration = true,
                ExistingUserType = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OAuth verification for provider: {Provider}", command.Provider.Name);
            return null;
        }
    }

    public async Task<AuthToken?> HandleCompleteOAuthRegistrationAsync(CompleteOAuthRegistrationCommand command)
    {
        try
        {
            _logger.LogInformation("Processing OAuth registration completion command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid OAuth registration completion command for email: {Email}", command.Email);
                return null;
            }

            // Validate temp token
            var tempToken = await _oauthTempTokenService.ValidateAndRetrieveTokenAsync(command.TempToken);
            if (tempToken == null)
            {
                _logger.LogWarning("Invalid or expired temp token for OAuth registration: {Email}", command.Email);
                return null;
            }

            // Create user based on type
            AuthenticatedUser? user;

            if (command.UserType == "Psychologist")
            {
                user = await _userContextService.CreateUserAsync(
                    command.Email,
                    null, // OAuth users don't have password
                    command.FullName,
                    "Psychologist",
                    command.ProfessionalLicense,
                    command.Specialties);
            }
            else
            {
                user = await _userContextService.CreateUserAsync(
                    command.Email,
                    null, // OAuth users don't have password
                    command.FullName,
                    "General");
            }

            if (user == null)
            {
                _logger.LogWarning("Failed to create OAuth user for email: {Email}", command.Email);
                return null;
            }

            // Remove temp token
            await _oauthTempTokenService.RemoveTokenAsync(tempToken.Token);

            _logger.LogInformation("OAuth user registered successfully: {UserId} - {Email}", user.Id, user.Email);

            // Generate JWT token
            var authToken = _tokenService.GenerateToken(user);

            // Update last login asynchronously
            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                try
                {
                    await scopedUserService.UpdateUserLastLoginAsync(user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for new OAuth user: {UserId}", user.Id);
                }
            });

            return authToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OAuth registration completion for email: {Email}", command.Email);
            return null;
        }
    }
}