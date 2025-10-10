namespace SoftFocusBackend.Library.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que representa las emociones utilizadas para filtrar y recomendar contenido
/// </summary>
public enum EmotionalTag
{
    /// <summary>
    /// Feliz, alegre, optimista
    /// </summary>
    Happy,

    /// <summary>
    /// Triste, melancólico, nostálgico
    /// </summary>
    Sad,

    /// <summary>
    /// Ansioso, estresado, preocupado
    /// </summary>
    Anxious,

    /// <summary>
    /// Calmado, relajado, en paz
    /// </summary>
    Calm,

    /// <summary>
    /// Energético, motivado, activo
    /// </summary>
    Energetic
}
