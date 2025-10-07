using SoftFocusBackend.Users.Application.ACL.Services;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;

namespace SoftFocusBackend.Users.Application.Internal.CommandServices;

public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDomainService _userDomainService;
    private readonly IAuthNotificationService _authNotificationService;
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly IGenericEmailService _emailService;
    private readonly ILogger<UserCommandService> _logger;

    public UserCommandService(
        IUserRepository userRepository,
        IUserDomainService userDomainService,
        IAuthNotificationService authNotificationService,
        ICloudinaryImageService cloudinaryImageService,
        IGenericEmailService emailService,
        ILogger<UserCommandService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userDomainService = userDomainService ?? throw new ArgumentNullException(nameof(userDomainService));
        _authNotificationService = authNotificationService ?? throw new ArgumentNullException(nameof(authNotificationService));
        _cloudinaryImageService = cloudinaryImageService ?? throw new ArgumentNullException(nameof(cloudinaryImageService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> HandleCreateUserAsync(CreateUserCommand command)
    {
        try
        {
            _logger.LogInformation("Processing create user command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid create user command for email: {Email}", command.Email);
                return null;
            }

            if (!await _userDomainService.IsEmailUniqueAsync(command.Email))
            {
                _logger.LogWarning("Email already exists: {Email}", command.Email);
                return null;
            }

            User user;
            if (command.IsPsychologist())
            {
                if (string.IsNullOrWhiteSpace(command.ProfessionalLicense) || 
                    command.Specialties == null || command.Specialties.Count == 0)
                {
                    _logger.LogWarning("Invalid psychologist data for email: {Email}", command.Email);
                    return null;
                }

                user = await _userDomainService.CreatePsychologistAsync(
                    command.Email, 
                    command.PasswordHash, 
                    command.FullName,
                    command.ProfessionalLicense, 
                    "Pending College Verification", 
                    command.Specialties, 
                    0);
            }
            else
            {
                user = await _userDomainService.CreateUserAsync(
                    command.Email, 
                    command.PasswordHash, 
                    command.FullName, 
                    command.UserType);
            }

            await _userRepository.AddAsync(user);

            await _authNotificationService.NotifyUserCreatedAsync(user.Id, user.Email, user.UserType.ToString());

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to: {Email}", user.Email);
                }
            });

            _logger.LogInformation("User created successfully: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user for email: {Email}", command.Email);
            return null;
        }
    }

    public async Task<User?> HandleUpdateUserProfileAsync(UpdateUserProfileCommand command)
    {
        try
        {
            _logger.LogInformation("Processing update user profile command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid update user profile command for user: {UserId}", command.UserId);
                return null;
            }

            var user = await _userRepository.FindByIdAsync(command.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", command.UserId);
                return null;
            }

            user.UpdateProfile(
                command.FullName,
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.Gender,
                command.Phone,
                command.Bio,
                command.Country,
                command.City,
                command.Interests,
                command.MentalHealthGoals);

            if (command.EmailNotifications.HasValue || command.PushNotifications.HasValue)
            {
                user.UpdateNotificationSettings(
                    command.EmailNotifications ?? user.EmailNotifications,
                    command.PushNotifications ?? user.PushNotifications);
            }

            // Upload profile image to Cloudinary if provided
            if (command.ProfileImageBytes != null && command.ProfileImageBytes.Length > 0 &&
                !string.IsNullOrWhiteSpace(command.ProfileImageFileName))
            {
                try
                {
                    var imageUrl = await _cloudinaryImageService.UploadImageAsync(
                        command.ProfileImageBytes,
                        command.ProfileImageFileName,
                        "softfocus/profiles/");
                    user.SetProfileImageUrl(imageUrl);
                    _logger.LogInformation("Profile image uploaded successfully for user: {UserId}", user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload profile image for user: {UserId}", command.UserId);
                    // Continue with profile update even if image upload fails
                }
            }
            else if (!string.IsNullOrWhiteSpace(command.ProfileImageUrl))
            {
                user.SetProfileImageUrl(command.ProfileImageUrl);
            }

            if (command.IsProfilePublic.HasValue)
            {
                user.IsProfilePublic = command.IsProfilePublic.Value;
                user.UpdatedAt = DateTime.UtcNow;
            }

            _userRepository.Update(user);

            await _authNotificationService.NotifyUserUpdatedAsync(user.Id, user.Email);

            _logger.LogInformation("User profile updated successfully: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<bool> HandleDeleteUserAsync(DeleteUserCommand command)
    {
        try
        {
            _logger.LogInformation("Processing delete user command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid delete user command for user: {UserId}", command.UserId);
                return false;
            }

            var user = await _userRepository.FindByIdAsync(command.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", command.UserId);
                return false;
            }

            if (!await _userDomainService.CanUserBeDeletedAsync(command.UserId))
            {
                _logger.LogWarning("User cannot be deleted due to business rules: {UserId}", command.UserId);
                return false;
            }

            if (command.HardDelete)
            {
                if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(user.ProfileImageUrl);
                            await _cloudinaryImageService.DeleteImageAsync(publicId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete profile image for user: {UserId}", user.Id);
                        }
                    });
                }

                _userRepository.Remove(user);
            }
            else
            {
                user.Deactivate();
                _userRepository.Update(user);
            }

            await _authNotificationService.NotifyUserDeletedAsync(user.Id, user.Email);

            _logger.LogInformation("User deleted successfully: {UserId} (Hard: {HardDelete})", user.Id, command.HardDelete);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", command.UserId);
            return false;
        }
    }
}