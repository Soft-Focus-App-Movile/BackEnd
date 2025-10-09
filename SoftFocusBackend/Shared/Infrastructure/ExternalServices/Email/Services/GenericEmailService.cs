 using System.Net;
using System.Net.Mail;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Configuration;
using Microsoft.Extensions.Options;

namespace SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;

public class GenericEmailService : IGenericEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<GenericEmailService> _logger;

    public GenericEmailService(IOptions<EmailSettings> emailSettings, ILogger<GenericEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName)
    {
        try
        {
            var subject = "¡Bienvenido a Soft Focus!";
            var htmlBody = CreateWelcomeEmailTemplate(fullName);

            await SendEmailInternalAsync(email, subject, htmlBody);
            
            _logger.LogInformation("Welcome email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken)
    {
        try
        {
            var subject = "Soft Focus - Restablecer contraseña";
            var htmlBody = CreatePasswordResetTemplate(fullName, resetToken);

            await SendEmailInternalAsync(email, subject, htmlBody);
            
            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
        }
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
    {
        try
        {
            var subject = "Soft Focus - Contraseña cambiada exitosamente";
            var htmlBody = CreatePasswordChangedTemplate(fullName);

            await SendEmailInternalAsync(email, subject, htmlBody);
            
            _logger.LogInformation("Password changed notification sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password changed notification to {Email}", email);
        }
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody, bool isHtml)
    {
        try
        {
            await SendEmailInternalAsync(email, subject, htmlBody);
            _logger.LogInformation("Custom email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send custom email to {Email}", email);
            throw;
        }
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = htmlBody;
        mailMessage.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
        smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
        smtpClient.EnableSsl = _emailSettings.EnableSsl;

        await smtpClient.SendMailAsync(mailMessage);
    }

    private static string CreateWelcomeEmailTemplate(string fullName)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Bienvenido a Soft Focus</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #667eea, #764ba2); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>Soft Focus</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Tu acompañamiento emocional</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #667eea; margin-top: 0;'>¡Bienvenido a Soft Focus, {fullName}!</h2>
                
                <p>¡Nos alegra tenerte con nosotros! Tu cuenta ha sido creada exitosamente.</p>
                
                <div style='background: #f0f4ff; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #374151;'>¿Qué puedes hacer ahora?</h3>
                    <ul style='margin: 10px 0; padding-left: 20px;'>
                        <li>Completa tu perfil personal</li>
                        <li>Realiza tu primer check-in emocional</li>
                        <li>Explora nuestra biblioteca de recursos</li>
                        <li>Conecta con un psicólogo si lo necesitas</li>
                    </ul>
                </div>
                
                <p>Estamos aquí para acompañarte en tu bienestar emocional.</p>
            </div>
        </body>
        </html>";
    }

    private static string CreatePasswordResetTemplate(string fullName, string resetToken)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Restablecer Contraseña - Soft Focus</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #f59e0b, #d97706); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>Soft Focus</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Restablecer Contraseña</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #f59e0b; margin-top: 0;'>Hola {fullName},</h2>
                
                <p>Recibimos una solicitud para restablecer tu contraseña en Soft Focus.</p>
                
                <div style='background: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #374151;'>Tu código de restablecimiento:</h3>
                    <p style='font-size: 24px; font-weight: bold; text-align: center; color: #f59e0b; margin: 10px 0;'>{resetToken}</p>
                </div>
                
                <p>Este código expira en 1 hora por seguridad.</p>
                <p>Si no solicitaste este cambio, puedes ignorar este email.</p>
            </div>
        </body>
        </html>";
    }

    private static string CreatePasswordChangedTemplate(string fullName)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Contraseña Cambiada - Soft Focus</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #10b981, #059669); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>Soft Focus</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Notificación de Seguridad</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #10b981; margin-top: 0;'>¡Contraseña cambiada exitosamente!</h2>
                
                <p>Hola <strong>{fullName}</strong>,</p>
                
                <p>Te confirmamos que tu contraseña en Soft Focus ha sido cambiada exitosamente.</p>
                
                <div style='background: #d1fae5; border-left: 4px solid #10b981; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #065f46;'>Detalles del cambio:</h4>
                    <p style='margin: 5px 0;'><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    <p style='margin: 5px 0;'><strong>Estado:</strong> Cambio exitoso</p>
                </div>
                
                <p>Si no realizaste este cambio, contacta inmediatamente con soporte.</p>
            </div>
        </body>
        </html>";
    }
}