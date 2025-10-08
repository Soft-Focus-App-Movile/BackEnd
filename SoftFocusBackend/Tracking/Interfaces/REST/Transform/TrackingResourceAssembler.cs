using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Transform;

public static class TrackingResourceAssembler
{
    public static TrackingSummaryResource ToSummaryResource(
        List<CheckIn> checkIns, 
        List<EmotionalCalendar> calendarEntries, 
        CheckIn? todayCheckIn = null)
    {
        var emotionalLevels = checkIns.Where(c => c.EmotionalLevel > 0).Select(c => c.EmotionalLevel).ToList();
        var energyLevels = checkIns.Where(c => c.EnergyLevel > 0).Select(c => c.EnergyLevel).ToList();
        var moodLevels = calendarEntries.Where(e => e.MoodLevel > 0).Select(e => e.MoodLevel).ToList();

        var allSymptoms = checkIns.SelectMany(c => c.Symptoms).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var allTags = calendarEntries.SelectMany(e => e.EmotionalTags).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

        return new TrackingSummaryResource
        {
            HasTodayCheckIn = todayCheckIn != null,
            TodayCheckIn = todayCheckIn != null ? CheckInResourceAssembler.ToResource(todayCheckIn) : null,
            TotalCheckIns = checkIns.Count,
            TotalEmotionalCalendarEntries = calendarEntries.Count,
            AverageEmotionalLevel = emotionalLevels.Any() ? Math.Round(emotionalLevels.Average(), 2) : 0,
            AverageEnergyLevel = energyLevels.Any() ? Math.Round(energyLevels.Average(), 2) : 0,
            AverageMoodLevel = moodLevels.Any() ? Math.Round(moodLevels.Average(), 2) : 0,
            MostCommonSymptoms = GetMostCommon(allSymptoms, 5),
            MostUsedEmotionalTags = GetMostCommon(allTags, 5)
        };
    }

    public static object ToTrackingDashboard(TrackingSummaryResource summary)
    {
        return new
        {
            success = true,
            data = new
            {
                summary,
                insights = GenerateInsights(summary)
            },
            timestamp = DateTime.UtcNow
        };
    }

    public static object ToPaginatedResponse<T>(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new
        {
            success = true,
            data = items,
            pagination = new
            {
                currentPage = pageNumber,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                hasNextPage = pageNumber * pageSize < totalCount,
                hasPreviousPage = pageNumber > 1
            },
            timestamp = DateTime.UtcNow
        };
    }

    private static List<string> GetMostCommon(List<string> items, int count)
    {
        return items
            .GroupBy(item => item.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToList();
    }

    private static object GenerateInsights(TrackingSummaryResource summary)
    {
        var insights = new List<string>();

        if (summary.AverageEmotionalLevel < 4)
            insights.Add("Your emotional levels have been lower than usual. Consider reaching out for support.");
        else if (summary.AverageEmotionalLevel > 7)
            insights.Add("Great job maintaining positive emotional levels!");

        if (summary.AverageEnergyLevel < 4)
            insights.Add("Your energy levels seem low. Focus on sleep and self-care.");
        else if (summary.AverageEnergyLevel > 7)
            insights.Add("You're maintaining good energy levels. Keep it up!");

        if (!summary.HasTodayCheckIn)
            insights.Add("Don't forget to complete your daily check-in to track your progress.");

        if (summary.TotalCheckIns < 7)
            insights.Add("Try to maintain consistent daily check-ins for better tracking.");

        return new { messages = insights };
    }
}