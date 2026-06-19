namespace SoftFocusBackend.Therapy.Infrastructure.ExternalServices.Agora.Configuration
{
    /// <summary>
    /// Agora.io project credentials. AppId is public; AppCertificate is secret and must be supplied
    /// via environment variable / user-secret, never committed.
    /// </summary>
    public class AgoraSettings
    {
        public string AppId { get; set; } = string.Empty;
        public string AppCertificate { get; set; } = string.Empty;

        /// <summary>How long issued RTC tokens stay valid, in seconds. Default: 1 hour.</summary>
        public uint TokenExpirationSeconds { get; set; } = 3600;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AppId) &&
                   !string.IsNullOrWhiteSpace(AppCertificate) &&
                   AppId.Length == 32 &&
                   AppCertificate.Length == 32;
        }
    }
}
