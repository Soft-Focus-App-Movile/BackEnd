namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    /// <summary>
    /// A participant ends the call (hang up) or leaves a group call.
    /// </summary>
    public class EndCallCommand
    {
        public string CallId { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
    }
}
