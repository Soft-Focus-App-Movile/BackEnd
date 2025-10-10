using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public interface IAssignedContentQueryService
{
    Task<List<ContentAssignment>> GetAssignedContentAsync(GetAssignedContentQuery query);
    Task<List<ContentAssignment>> GetAssignmentsByPsychologistAsync(string psychologistId, string? patientId = null);
}
