namespace SoftFocusBackend.Therapy.Interfaces.REST.Resources
{
    /// <summary>
    /// Body for POST /api/v1/calls/initiate.
    /// </summary>
    public class InitiateCallRequest
    {
        /// <summary>"Audio" or "Video". Defaults to "Video".</summary>
        public string CallType { get; set; } = "Video";

        /// <summary>
        /// "Direct" or "Group". Only meaningful for psychologists. Patients always place a Direct
        /// call to their psychologist regardless of this value.
        /// </summary>
        public string Mode { get; set; } = "Direct";

        /// <summary>
        /// For a psychologist placing a Direct call: the patient's user id to call. Ignored for
        /// Group calls and for patient callers.
        /// </summary>
        public string? TargetUserId { get; set; }
    }
}
