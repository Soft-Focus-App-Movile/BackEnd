using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tracking.Domain.Model.Aggregates;

public class CheckIn : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("emotionalLevel")]
    public int EmotionalLevel { get; set; }

    [BsonElement("symptoms")]
    public List<string> Symptoms { get; set; } = new();

    [BsonElement("notes")]
    public string Notes { get; set; } = string.Empty;

    [BsonElement("moodDescription")]
    public string MoodDescription { get; set; } = string.Empty;

    [BsonElement("sleepHours")]
    public decimal SleepHours { get; set; }

    [BsonElement("energyLevel")]
    public int EnergyLevel { get; set; }

    [BsonElement("completedAt")]
    public DateTime CompletedAt { get; set; }

    public void UpdateEmotionalState(EmotionalLevel emotionalLevel, EnergyLevel energyLevel, 
        MoodDescription moodDescription)
    {
        EmotionalLevel = emotionalLevel;
        EnergyLevel = energyLevel;
        MoodDescription = moodDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSleepInfo(SleepHours sleepHours)
    {
        SleepHours = sleepHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSymptoms(Symptoms symptoms)
    {
        Symptoms = symptoms;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(CheckInNotes notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCompletedToday()
    {
        return CompletedAt.Date == DateTime.UtcNow.Date;
    }

    public void ValidateForCreation()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("User ID is required");

        if (EmotionalLevel < 1 || EmotionalLevel > 10)
            throw new ArgumentException("Emotional level must be between 1 and 10");

        if (EnergyLevel < 1 || EnergyLevel > 10)
            throw new ArgumentException("Energy level must be between 1 and 10");
    }
}