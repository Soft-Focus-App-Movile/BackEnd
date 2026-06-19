using SoftFocusBackend.Therapy.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    /// <summary>
    /// Everything a client needs to join the Agora channel after initiating or answering a call.
    /// </summary>
    public record CallAccessResult(
        CallSession Session,
        string AppId,
        string Token,
        string UserAccount);
}
