using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;

namespace SoftFocusBackend.Notification.Infrastructure.ExternalServices;

public class EmailNotificationService
{
    private readonly IGenericEmailService _emailService;

    public EmailNotificationService(IGenericEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<bool> SendEmailNotificationAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            await _emailService.SendEmailAsync(to, subject, body, isHtml);
            return true;
        }
        catch
        {
            return false;
        }
    }
}