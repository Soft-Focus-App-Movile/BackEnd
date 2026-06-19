namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    /// <summary>
    /// An invitee accepts or rejects an incoming call.
    /// </summary>
    public class RespondToCallCommand
    {
        public string CallId { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public bool Accept { get; init; }
    }
}
