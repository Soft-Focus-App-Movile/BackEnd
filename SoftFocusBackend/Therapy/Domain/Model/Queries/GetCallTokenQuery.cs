namespace SoftFocusBackend.Therapy.Domain.Model.Queries
{
    /// <summary>Re-issue an Agora token for a participant of an existing call (e.g. token renewal).</summary>
    public class GetCallTokenQuery
    {
        public string CallId { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
    }
}
