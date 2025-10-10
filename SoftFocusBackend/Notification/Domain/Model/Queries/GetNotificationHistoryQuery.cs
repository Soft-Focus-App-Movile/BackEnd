namespace SoftFocusBackend.Notification.Domain.Model.Queries;

public record GetNotificationHistoryQuery(
    string UserId,
    string? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
);