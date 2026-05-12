using MongoDB.Bson;
using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tests.Helpers;

/// <summary>
/// Factory de objetos de dominio para tests. Centraliza la creación de
/// instancias evitando duplicar lógica de construcción en cada prueba.
/// </summary>
public static class DomainObjectBuilder
{
    // ─── IDs reutilizables ────────────────────────────────────────────────
    public static readonly string PsychologistId  = ObjectId.GenerateNewId().ToString();
    public static readonly string PatientId       = ObjectId.GenerateNewId().ToString();
    public static readonly string RelationshipId  = ObjectId.GenerateNewId().ToString();

    // ─── TherapeuticRelationship ──────────────────────────────────────────

    /// <summary>
    /// Crea una relación terapéutica activa (estado por defecto del constructor).
    /// </summary>
    public static TherapeuticRelationship CreateActiveRelationship(
        string? psychologistId = null,
        string? patientId      = null)
    {
        var code = ConnectionCode.Create("ABCD1234");
        return new TherapeuticRelationship(
            psychologistId ?? PsychologistId,
            code,
            patientId ?? PatientId);
    }

    /// <summary>
    /// Crea una relación terminada.
    /// </summary>
    public static TherapeuticRelationship CreateTerminatedRelationship()
    {
        var rel = CreateActiveRelationship();
        rel.Terminate();
        return rel;
    }

    // ─── ConnectionCode ───────────────────────────────────────────────────

    public static ConnectionCode CreateConnectionCode(string value = "ABCD1234")
        => ConnectionCode.Create(value);

    // ─── ChatMessage ──────────────────────────────────────────────────────

    public static ChatMessage CreateChatMessage(
        string? relationshipId = null,
        string? senderId       = null,
        string? receiverId     = null,
        string  text           = "Hola, ¿cómo estás?",
        string  messageType    = "text")
    {
        var content = MessageContent.Create(text);
        return new ChatMessage(
            relationshipId ?? RelationshipId,
            senderId       ?? PsychologistId,
            receiverId     ?? PatientId,
            content,
            messageType);
    }

    public static List<ChatMessage> CreateChatMessageList(int count = 5)
        => Enumerable.Range(0, count)
            .Select(i => CreateChatMessage(text: $"Mensaje {i}"))
            .ToList();

    // ─── CrisisAlert ──────────────────────────────────────────────────────

    public static CrisisAlert CreateCrisisAlert(
        string?        patientId       = null,
        string?        psychologistId  = null,
        AlertSeverity  severity        = AlertSeverity.Critical,
        string         triggerSource   = "MANUAL_BUTTON")
    {
        return new CrisisAlert(
            patientId:      patientId      ?? PatientId,
            psychologistId: psychologistId ?? PsychologistId,
            severity:       severity,
            triggerSource:  triggerSource,
            triggerReason:  "Usuario presionó el botón de crisis");
    }

    public static CrisisAlert CreateCrisisAlertWithLocation(
        double lat = -12.0464, double lon = -77.0428)
    {
        return new CrisisAlert(
            patientId:      PatientId,
            psychologistId: PsychologistId,
            severity:       AlertSeverity.Critical,
            triggerSource:  "MANUAL_BUTTON",
            location:       new Location(lat, lon));
    }

    public static CrisisAlert CreateCrisisAlertWithEmotion(string emotion = "sad")
    {
        return new CrisisAlert(
            patientId:        PatientId,
            psychologistId:   PsychologistId,
            severity:         AlertSeverity.High,
            triggerSource:    "EMOTION_ANALYSIS",
            emotionalContext: new EmotionalContext(emotion, DateTime.UtcNow, "Facial Analysis"));
    }

    // ─── User (paciente simulado) ─────────────────────────────────────────

    /// <summary>
    /// Crea un User mínimo suficiente para los tests sin depender de
    /// constructores internos del bounded context Users.
    /// </summary>
    public static User CreatePatientUser(
        string? id        = null,
        string  firstName = "Ana",
        string  lastName  = "García")
    {
        // Usamos reflexión para asignar Id porque BaseEntity lo tiene privado.
        var user = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;
        SetPrivateProperty(user, "Id",        id ?? PatientId);
        SetPrivateProperty(user, "FirstName", firstName);
        SetPrivateProperty(user, "LastName",  lastName);
        SetPrivateProperty(user, "Email",     "ana@test.com");
        return user;
    }

    // ─── helpers privados ─────────────────────────────────────────────────

    private static void SetPrivateProperty(object obj, string propName, object? value)
    {
        var prop = obj.GetType()
            .GetProperty(propName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.FlattenHierarchy);

        if (prop?.CanWrite == true)
            prop.SetValue(obj, value);
        else
        {
            // fallback: campo backing
            var field = obj.GetType()
                .GetField($"<{propName}>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.FlattenHierarchy);
            field?.SetValue(obj, value);
        }
    }
}