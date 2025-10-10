using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para obtener contenido filtrado por una emoción específica
/// </summary>
public class GetContentByEmotionQuery
{
    /// <summary>
    /// Emoción para filtrar contenido
    /// </summary>
    public EmotionalTag Emotion { get; set; }

    /// <summary>
    /// Tipo de contenido a buscar (opcional, null = todos)
    /// </summary>
    public ContentType? ContentType { get; set; }

    /// <summary>
    /// Límite de resultados (default: 20)
    /// </summary>
    public int Limit { get; set; } = 20;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public GetContentByEmotionQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public GetContentByEmotionQuery(
        EmotionalTag emotion,
        ContentType? contentType = null,
        int limit = 20)
    {
        Emotion = emotion;
        ContentType = contentType;
        Limit = limit;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (Limit <= 0 || Limit > 100)
            throw new ArgumentException("Limit debe estar entre 1 y 100");
    }
}
