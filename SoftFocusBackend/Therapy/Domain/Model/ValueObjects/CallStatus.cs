namespace SoftFocusBackend.Therapy.Domain.Model.ValueObjects
{
    /// <summary>
    /// Lifecycle of a call session.
    /// </summary>
    public enum CallStatus
    {
        /// Created by the caller, invitees notified, nobody has answered yet.
        Ringing,

        /// At least one invitee accepted; the call is in progress.
        Ongoing,

        /// The call ended normally after being answered.
        Ended,

        /// Every invitee rejected the call (direct calls).
        Rejected,

        /// Nobody answered before the call was cancelled/timed out.
        Missed,

        /// The caller hung up/cancelled before anyone answered.
        Cancelled
    }
}
