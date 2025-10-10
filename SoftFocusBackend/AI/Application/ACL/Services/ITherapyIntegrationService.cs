namespace SoftFocusBackend.AI.Application.ACL.Services;

public interface ITherapyIntegrationService
{
    Task<List<string>> GetCurrentTherapyGoalsAsync(string userId);
    Task<List<string>> GetAssignedExercisesAsync(string userId);
}
