using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Commands;

/// <summary>
/// Command para agregar un contenido a favoritos de un usuario
/// </summary>
public class AddFavoriteCommand
{
    /// <summary>
    /// ID del usuario (obtenido del token JWT)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ExternalId del contenido a agregar (ej: "tmdb-movie-27205")
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public AddFavoriteCommand() { }

    /// <summary>
    /// Crea un nuevo comando
    /// </summary>
    public AddFavoriteCommand(string userId, string contentId, ContentType contentType)
    {
        UserId = userId;
        ContentId = contentId;
        ContentType = contentType;
    }

    /// <summary>
    /// Valida que el comando tenga datos válidos
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId no puede estar vacío");

        if (string.IsNullOrWhiteSpace(ContentId))
            throw new ArgumentException("ContentId no puede estar vacío");
    }
}
