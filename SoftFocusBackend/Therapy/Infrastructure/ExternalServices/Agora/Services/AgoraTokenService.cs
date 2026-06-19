using AgoraIO.Media;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Services;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices.Agora.Configuration;

namespace SoftFocusBackend.Therapy.Infrastructure.ExternalServices.Agora.Services
{
    /// <summary>
    /// Generates Agora RTC tokens (AccessToken2 / version "007") using the vendored
    /// <see cref="RtcTokenBuilder2"/>. All call participants join as publishers.
    /// </summary>
    public class AgoraTokenService : IAgoraTokenService
    {
        private readonly AgoraSettings _settings;
        private readonly ILogger<AgoraTokenService> _logger;

        public AgoraTokenService(IOptions<AgoraSettings> settings, ILogger<AgoraTokenService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public string AppId => _settings.AppId;

        public string GenerateRtcToken(string channelName, string userAccount, CallType callType)
        {
            if (!_settings.IsValid())
            {
                _logger.LogError(
                    "AgoraSettings are not configured correctly. AppId/AppCertificate must each be 32 hex chars.");
                throw new InvalidOperationException(
                    "Agora is not configured. Set AgoraSettings:AppId and AgoraSettings:AppCertificate.");
            }

            var expire = _settings.TokenExpirationSeconds;

            // Publisher role: the participant can both subscribe and publish audio/video.
            var token = RtcTokenBuilder2.buildTokenWithUserAccount(
                _settings.AppId,
                _settings.AppCertificate,
                channelName,
                userAccount,
                RtcTokenBuilder2.Role.RolePublisher,
                expire,
                expire);

            if (string.IsNullOrEmpty(token))
            {
                // build() returns "" only when AppId/AppCertificate are not valid 32-char hex strings.
                throw new InvalidOperationException(
                    "Failed to generate Agora token. Verify AppId and AppCertificate are valid 32-character hex strings.");
            }

            return token;
        }
    }
}
