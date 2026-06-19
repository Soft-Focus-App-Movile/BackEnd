using SoftFocusBackend.Therapy.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Domain.Repositories
{
    public interface ICallSessionRepository
    {
        Task<CallSession?> GetByIdAsync(string id);
        Task AddAsync(CallSession callSession);
        Task UpdateAsync(CallSession callSession);

        /// <summary>Calls where the given user was the caller or an invitee, newest first.</summary>
        Task<IEnumerable<CallSession>> GetByParticipantIdAsync(string userId, int page, int size);
    }
}
