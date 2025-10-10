namespace SoftFocusBackend.Library.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que encapsula toda la metadata de un contenido multimedia
/// Soporta películas, series, música, videos y lugares
/// </summary>
public class ContentMetadata
{
    // Propiedades comunes
    public string Title { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string BackdropUrl { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();

    // Propiedades para música (Spotify)
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public string SpotifyUrl { get; set; } = string.Empty;

    // Propiedades para videos (YouTube)
    public string ChannelName { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;

    // Propiedades para lugares (Foursquare)
    public string Category { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Distance { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ContentMetadata() { }

    /// <summary>
    /// Crea metadata para una película/serie de TMDB
    /// </summary>
    public static ContentMetadata CreateForMovie(
        string title,
        string overview,
        string posterUrl,
        string backdropUrl,
        double rating,
        string duration,
        string trailerUrl,
        List<string> genres)
    {
        return new ContentMetadata
        {
            Title = title,
            Overview = overview,
            PosterUrl = posterUrl,
            BackdropUrl = backdropUrl,
            Rating = rating,
            Duration = duration,
            TrailerUrl = trailerUrl,
            Genres = genres
        };
    }

    /// <summary>
    /// Crea metadata para una canción de Spotify
    /// </summary>
    public static ContentMetadata CreateForMusic(
        string title,
        string artist,
        string album,
        string posterUrl,
        string duration,
        string previewUrl,
        string spotifyUrl)
    {
        return new ContentMetadata
        {
            Title = title,
            Artist = artist,
            Album = album,
            PosterUrl = posterUrl,
            Duration = duration,
            PreviewUrl = previewUrl,
            SpotifyUrl = spotifyUrl
        };
    }

    /// <summary>
    /// Crea metadata para un video de YouTube
    /// </summary>
    public static ContentMetadata CreateForVideo(
        string title,
        string overview,
        string thumbnailUrl,
        string channelName,
        string youtubeUrl)
    {
        return new ContentMetadata
        {
            Title = title,
            Overview = overview,
            ThumbnailUrl = thumbnailUrl,
            ChannelName = channelName,
            YouTubeUrl = youtubeUrl
        };
    }

    /// <summary>
    /// Crea metadata para un lugar de Foursquare
    /// </summary>
    public static ContentMetadata CreateForPlace(
        string name,
        string category,
        string address,
        double latitude,
        double longitude,
        int distance,
        double rating,
        string photoUrl)
    {
        return new ContentMetadata
        {
            Title = name,
            Category = category,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            Distance = distance,
            Rating = rating,
            PhotoUrl = photoUrl
        };
    }
}
