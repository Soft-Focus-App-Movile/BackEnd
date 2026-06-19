namespace SoftFocusBackend.Therapy.Domain.Model.Queries
{
    /// <summary>Paginated call history for a user (as caller or invitee), newest first.</summary>
    public class GetCallHistoryQuery
    {
        public string UserId { get; init; } = string.Empty;
        public int Page { get; init; } = 1;
        public int Size { get; init; } = 20;
    }
}
