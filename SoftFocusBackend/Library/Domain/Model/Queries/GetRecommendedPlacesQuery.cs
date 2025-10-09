using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para obtener lugares recomendados basados en clima y ubicación
/// </summary>
public class GetRecommendedPlacesQuery
{
    /// <summary>
    /// Latitud de la ubicación del usuario
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitud de la ubicación del usuario
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Emoción opcional para refinar recomendaciones
    /// </summary>
    public EmotionalTag? Emotion { get; set; }

    /// <summary>
    /// Radio de búsqueda en metros (default: 5000)
    /// </summary>
    public int Radius { get; set; } = 5000;

    /// <summary>
    /// Límite de resultados (default: 10)
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public GetRecommendedPlacesQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public GetRecommendedPlacesQuery(
        double latitude,
        double longitude,
        EmotionalTag? emotion = null,
        int radius = 5000,
        int limit = 10)
    {
        Latitude = latitude;
        Longitude = longitude;
        Emotion = emotion;
        Radius = radius;
        Limit = limit;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (Latitude < -90 || Latitude > 90)
            throw new ArgumentException("Latitude debe estar entre -90 y 90");

        if (Longitude < -180 || Longitude > 180)
            throw new ArgumentException("Longitude debe estar entre -180 y 180");

        if (Radius <= 0 || Radius > 50000)
            throw new ArgumentException("Radius debe estar entre 1 y 50000 metros");

        if (Limit <= 0 || Limit > 50)
            throw new ArgumentException("Limit debe estar entre 1 y 50");
    }
}
