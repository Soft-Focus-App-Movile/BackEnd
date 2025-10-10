using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para obtener los favoritos de un usuario
/// </summary>
public class GetFavoritesQuery
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Filtro opcional por tipo de contenido
    /// </summary>
    public ContentType? ContentTypeFilter { get; set; }

    /// <summary>
    /// Filtro opcional por emoción
    /// </summary>
    public EmotionalTag? EmotionFilter { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public GetFavoritesQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public GetFavoritesQuery(
        string userId,
        ContentType? contentTypeFilter = null,
        EmotionalTag? emotionFilter = null)
    {
        UserId = userId;
        ContentTypeFilter = contentTypeFilter;
        EmotionFilter = emotionFilter;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId no puede estar vacío");
    }
}
