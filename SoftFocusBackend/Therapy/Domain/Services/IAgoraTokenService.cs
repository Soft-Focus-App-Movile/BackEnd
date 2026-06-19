using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Services
{
    /// <summary>
    /// Issues Agora RTC tokens so a client can join a channel. Implemented in Infrastructure using
    /// the Agora App ID + App Certificate.
    /// </summary>
    public interface IAgoraTokenService
    {
        /// <summary>The public Agora App ID (safe to return to clients).</summary>
        string AppId { get; }

        /// <summary>
        /// Builds an RTC token bound to <paramref name="channelName"/> and <paramref name="userAccount"/>.
        /// Every call participant joins as a publisher (they all send audio/video).
        /// </summary>
        string GenerateRtcToken(string channelName, string userAccount, CallType callType);
    }
}
