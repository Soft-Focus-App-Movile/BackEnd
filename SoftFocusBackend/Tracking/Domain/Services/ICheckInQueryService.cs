using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Queries;

namespace SoftFocusBackend.Tracking.Domain.Services;

public interface ICheckInQueryService
{
    Task<CheckIn?> HandleGetCheckInByIdAsync(GetCheckInByIdQuery query);
    Task<CheckIn?> HandleGetTodayCheckInAsync(GetTodayCheckInQuery query);
    Task<List<CheckIn>> HandleGetUserCheckInsAsync(GetUserCheckInsQuery query);
}