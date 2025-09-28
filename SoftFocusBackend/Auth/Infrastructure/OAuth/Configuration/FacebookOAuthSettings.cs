namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Configuration;

public class FacebookOAuthSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = { "email", "public_profile" };

    public bool IsValid => 
        !string.IsNullOrEmpty(AppId) && 
        !string.IsNullOrEmpty(AppSecret);
}