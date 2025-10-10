namespace SoftFocusBackend.Library.Domain.Model.Commands;

/// <summary>
/// Command para eliminar un contenido de favoritos de un usuario
/// </summary>
public class RemoveFavoriteCommand
{
    /// <summary>
    /// ID del usuario (obtenido del token JWT)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID del favorito a eliminar
    /// </summary>
    public string FavoriteId { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public RemoveFavoriteCommand() { }

    /// <summary>
    /// Crea un nuevo comando
    /// </summary>
    public RemoveFavoriteCommand(string userId, string favoriteId)
    {
        UserId = userId;
        FavoriteId = favoriteId;
    }

    /// <summary>
    /// Valida que el comando tenga datos válidos
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId no puede estar vacío");

        if (string.IsNullOrWhiteSpace(FavoriteId))
            throw new ArgumentException("FavoriteId no puede estar vacío");
    }
}
