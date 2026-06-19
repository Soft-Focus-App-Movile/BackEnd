using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    /// <summary>
    /// Starts a call. The caller is taken from the authenticated user.
    /// - Patient caller: always a Direct call to their psychologist (TargetUserId/Mode ignored).
    /// - Psychologist caller + Mode.Direct: Direct call to TargetUserId (a patient).
    /// - Psychologist caller + Mode.Group: Group call to all of their active patients.
    /// </summary>
    public class InitiateCallCommand
    {
        public string CallerId { get; init; } = string.Empty;
        public CallType CallType { get; init; } = CallType.Video;
        public CallMode Mode { get; init; } = CallMode.Direct;
        public string? TargetUserId { get; init; }
    }
}
