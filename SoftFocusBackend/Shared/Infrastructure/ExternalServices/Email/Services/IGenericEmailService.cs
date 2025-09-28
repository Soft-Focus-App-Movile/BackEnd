namespace SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;

public interface IGenericEmailService
{
    Task SendWelcomeEmailAsync(string email, string fullName);
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
    Task SendEmailAsync(string email, string subject, string htmlBody);
}