using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para buscar contenido multimedia en APIs externas
/// </summary>
public class SearchContentQuery
{
    /// <summary>
    /// Término de búsqueda
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido a buscar
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Filtro opcional por emoción
    /// </summary>
    public EmotionalTag? EmotionFilter { get; set; }

    /// <summary>
    /// Límite de resultados (default: 20)
    /// </summary>
    public int Limit { get; set; } = 20;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public SearchContentQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public SearchContentQuery(
        string query,
        ContentType contentType,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        Query = query;
        ContentType = contentType;
        EmotionFilter = emotionFilter;
        Limit = limit;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Query))
            throw new ArgumentException("Query no puede estar vacío");

        if (Limit <= 0 || Limit > 100)
            throw new ArgumentException("Limit debe estar entre 1 y 100");
    }
}
