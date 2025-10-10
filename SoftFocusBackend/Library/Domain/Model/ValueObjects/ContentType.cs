namespace SoftFocusBackend.Library.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que representa los tipos de contenido disponibles en la biblioteca multimedia
/// </summary>
public enum ContentType
{
    /// <summary>
    /// Película (TMDB)
    /// </summary>
    Movie,

    /// <summary>
    /// Serie de TV (TMDB)
    /// </summary>
    Series,

    /// <summary>
    /// Música/Canción (Spotify)
    /// </summary>
    Music,

    /// <summary>
    /// Video de bienestar/meditación (YouTube)
    /// </summary>
    Video,

    /// <summary>
    /// Lugar físico recomendado (Foursquare)
    /// </summary>
    Place
}
