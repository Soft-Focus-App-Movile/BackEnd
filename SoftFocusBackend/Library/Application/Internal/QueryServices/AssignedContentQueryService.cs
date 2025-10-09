using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Repositories;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public class AssignedContentQueryService : IAssignedContentQueryService
{
    private readonly IContentAssignmentRepository _assignmentRepository;
    private readonly ILogger<AssignedContentQueryService> _logger;

    public AssignedContentQueryService(
        IContentAssignmentRepository assignmentRepository,
        ILogger<AssignedContentQueryService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<List<ContentAssignment>> GetAssignedContentAsync(GetAssignedContentQuery query)
    {
        query.Validate();

        if (query.CompletedFilter.HasValue)
        {
            return query.CompletedFilter.Value
                ? (await _assignmentRepository.FindCompletedByPatientIdAsync(query.PatientId)).ToList()
                : (await _assignmentRepository.FindPendingByPatientIdAsync(query.PatientId)).ToList();
        }

        return (await _assignmentRepository.FindByPatientIdAsync(query.PatientId)).ToList();
    }

    public async Task<List<ContentAssignment>> GetAssignmentsByPsychologistAsync(
        string psychologistId,
        string? patientId = null)
    {
        if (!string.IsNullOrEmpty(patientId))
        {
            return (await _assignmentRepository.FindByPsychologistAndPatientAsync(
                psychologistId, patientId)).ToList();
        }

        return (await _assignmentRepository.FindByPsychologistIdAsync(psychologistId)).ToList();
    }
}
