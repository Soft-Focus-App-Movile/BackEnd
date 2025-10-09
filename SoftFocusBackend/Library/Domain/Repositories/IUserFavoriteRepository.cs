using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar los favoritos de usuarios
/// </summary>
public interface IUserFavoriteRepository : IBaseRepository<UserFavorite>
{
    /// <summary>
    /// Obtiene todos los favoritos de un usuario
    /// </summary>
    Task<IEnumerable<UserFavorite>> FindByUserIdAsync(string userId);

    /// <summary>
    /// Obtiene favoritos de un usuario filtrados por tipo de contenido
    /// </summary>
    Task<IEnumerable<UserFavorite>> FindByUserIdAndTypeAsync(
        string userId,
        ContentType contentType);

    /// <summary>
    /// Obtiene favoritos de un usuario filtrados por emoción
    /// </summary>
    Task<IEnumerable<UserFavorite>> FindByUserIdAndEmotionAsync(
        string userId,
        EmotionalTag emotion);

    /// <summary>
    /// Busca un favorito específico por usuario y contenido
    /// </summary>
    Task<UserFavorite?> FindByUserAndContentAsync(
        string userId,
        string contentId);

    /// <summary>
    /// Verifica si un contenido ya está en favoritos de un usuario
    /// </summary>
    Task<bool> ExistsAsync(string userId, string contentId);

    /// <summary>
    /// Cuenta los favoritos de un usuario
    /// </summary>
    Task<int> CountByUserIdAsync(string userId);
}
