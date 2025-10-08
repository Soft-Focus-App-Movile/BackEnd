using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Queries;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tracking.Application.Internal.QueryServices;

public class CheckInQueryService : ICheckInQueryService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger<CheckInQueryService> _logger;

    public CheckInQueryService(
        ICheckInRepository checkInRepository,
        ILogger<CheckInQueryService> logger)
    {
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CheckIn?> HandleGetCheckInByIdAsync(GetCheckInByIdQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get check-in by id query: {CheckInId}", query.CheckInId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get check-in by id query: {CheckInId}", query.CheckInId);
                return null;
            }

            var checkIn = await _checkInRepository.FindByIdAsync(query.CheckInId);
            
            _logger.LogInformation("Check-in query completed: {CheckInId} - Found: {Found}", query.CheckInId, checkIn != null);
            return checkIn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting check-in by id: {CheckInId}", query.CheckInId);
            return null;
        }
    }

    public async Task<CheckIn?> HandleGetTodayCheckInAsync(GetTodayCheckInQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get today check-in query for user: {UserId}", query.UserId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get today check-in query for user: {UserId}", query.UserId);
                return null;
            }

            var checkIn = await _checkInRepository.FindTodayCheckInByUserIdAsync(query.UserId);
            
            _logger.LogInformation("Today check-in query completed for user: {UserId} - Found: {Found}", query.UserId, checkIn != null);
            return checkIn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today check-in for user: {UserId}", query.UserId);
            return null;
        }
    }

    public async Task<List<CheckIn>> HandleGetUserCheckInsAsync(GetUserCheckInsQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get user check-ins query for user: {UserId}", query.UserId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get user check-ins query for user: {UserId}", query.UserId);
                return new List<CheckIn>();
            }

            var checkIns = await _checkInRepository.FindByUserIdAsync(query.UserId, query.StartDate, query.EndDate, query.PageNumber, query.PageSize);
            
            _logger.LogInformation("User check-ins query completed for user: {UserId} - Count: {Count}", query.UserId, checkIns.Count);
            return checkIns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user check-ins for user: {UserId}", query.UserId);
            return new List<CheckIn>();
        }
    }
}