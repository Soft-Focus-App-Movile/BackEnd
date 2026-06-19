namespace SoftFocusBackend.Therapy.Domain.Model.ValueObjects
{
    /// <summary>
    /// Direct = 1:1 call (patient ↔ psychologist).
    /// Group = a psychologist calls all of their active patients into a single channel.
    /// </summary>
    public enum CallMode
    {
        Direct,
        Group
    }
}
