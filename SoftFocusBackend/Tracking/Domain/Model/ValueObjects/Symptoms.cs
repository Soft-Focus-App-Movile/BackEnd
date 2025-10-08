namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record Symptoms
{
    public List<string> Value { get; init; }

    public Symptoms(List<string> symptoms)
    {
        if (symptoms == null)
            throw new ArgumentException("Symptoms list cannot be null.", nameof(symptoms));

        if (symptoms.Count > 20)
            throw new ArgumentException("Cannot have more than 20 symptoms.", nameof(symptoms));

        Value = symptoms.Where(s => !string.IsNullOrWhiteSpace(s))
                       .Select(s => s.Trim())
                       .ToList();
    }

    public static implicit operator List<string>(Symptoms symptoms) => symptoms.Value;
    public static implicit operator Symptoms(List<string> symptoms) => new(symptoms);
}