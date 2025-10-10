using SoftFocusBackend.Library.Domain.Model.Commands;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

/// <summary>
/// Servicio de aplicación para comandos relacionados con favoritos
/// </summary>
public interface IFavoriteCommandService
{
    /// <summary>
    /// Agrega un contenido a favoritos
    /// </summary>
    Task<string> AddFavoriteAsync(AddFavoriteCommand command);

    /// <summary>
    /// Elimina un contenido de favoritos
    /// </summary>
    Task RemoveFavoriteAsync(RemoveFavoriteCommand command);
}
