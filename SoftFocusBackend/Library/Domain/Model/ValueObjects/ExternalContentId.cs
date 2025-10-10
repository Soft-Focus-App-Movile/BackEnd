namespace SoftFocusBackend.Library.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que representa un identificador único de contenido externo
/// Formato: {provider}-{type}-{id} (ej: "tmdb-movie-27205", "spotify-track-3n3Ppam7vgaVa1iaRUc9Lp")
/// </summary>
public class ExternalContentId
{
    public string Value { get; }

    private ExternalContentId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea un ExternalContentId para TMDB
    /// </summary>
    public static ExternalContentId CreateTmdbId(string tmdbId, ContentType contentType)
    {
        if (string.IsNullOrWhiteSpace(tmdbId))
            throw new ArgumentException("TMDB ID no puede estar vacío", nameof(tmdbId));

        if (contentType != ContentType.Movie && contentType != ContentType.Series)
            throw new ArgumentException("ContentType debe ser Movie o Series para TMDB", nameof(contentType));

        var typeString = contentType == ContentType.Movie ? "movie" : "tv";
        return new ExternalContentId($"tmdb-{typeString}-{tmdbId}");
    }

    /// <summary>
    /// Crea un ExternalContentId para Spotify
    /// </summary>
    public static ExternalContentId CreateSpotifyId(string trackId)
    {
        if (string.IsNullOrWhiteSpace(trackId))
            throw new ArgumentException("Spotify track ID no puede estar vacío", nameof(trackId));

        return new ExternalContentId($"spotify-track-{trackId}");
    }

    /// <summary>
    /// Crea un ExternalContentId para YouTube
    /// </summary>
    public static ExternalContentId CreateYouTubeId(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
            throw new ArgumentException("YouTube video ID no puede estar vacío", nameof(videoId));

        return new ExternalContentId($"youtube-video-{videoId}");
    }

    /// <summary>
    /// Crea un ExternalContentId para Foursquare
    /// </summary>
    public static ExternalContentId CreateFoursquareId(string venueId)
    {
        if (string.IsNullOrWhiteSpace(venueId))
            throw new ArgumentException("Foursquare venue ID no puede estar vacío", nameof(venueId));

        return new ExternalContentId($"fsq-venue-{venueId}");
    }

    /// <summary>
    /// Crea un ExternalContentId desde un string existente
    /// </summary>
    public static ExternalContentId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ExternalContentId no puede estar vacío", nameof(value));

        // Validar formato básico
        var parts = value.Split('-');
        if (parts.Length < 3)
            throw new ArgumentException("ExternalContentId debe tener formato: provider-type-id", nameof(value));

        return new ExternalContentId(value);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is ExternalContentId other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
