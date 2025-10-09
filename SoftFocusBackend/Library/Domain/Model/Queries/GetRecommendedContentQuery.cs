using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para obtener contenido recomendado basado en emoción actual y/o clima
/// </summary>
public class GetRecommendedContentQuery
{
    /// <summary>
    /// ID del usuario (para obtener emoción desde Tracking Context)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido a recomendar (opcional, null = todos)
    /// </summary>
    public ContentType? ContentType { get; set; }

    /// <summary>
    /// Límite de resultados (default: 10)
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Latitud para recomendaciones basadas en clima (opcional)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitud para recomendaciones basadas en clima (opcional)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public GetRecommendedContentQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public GetRecommendedContentQuery(
        string userId,
        ContentType? contentType = null,
        int limit = 10,
        double? latitude = null,
        double? longitude = null)
    {
        UserId = userId;
        ContentType = contentType;
        Limit = limit;
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId no puede estar vacío");

        if (Limit <= 0 || Limit > 100)
            throw new ArgumentException("Limit debe estar entre 1 y 100");

        if (Latitude.HasValue && (Latitude.Value < -90 || Latitude.Value > 90))
            throw new ArgumentException("Latitude debe estar entre -90 y 90");

        if (Longitude.HasValue && (Longitude.Value < -180 || Longitude.Value > 180))
            throw new ArgumentException("Longitude debe estar entre -180 y 180");
    }
}
