using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

/// <summary>
/// Implementación del servicio de comandos de favoritos
/// </summary>
public class FavoriteCommandService : IFavoriteCommandService
{
    private readonly IUserFavoriteRepository _favoriteRepository;
    private readonly IContentItemRepository _contentRepository;
    private readonly IContentSearchService _searchService;
    private readonly IContentCacheService _cacheService;
    private readonly IUserIntegrationService _userIntegration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FavoriteCommandService> _logger;

    public FavoriteCommandService(
        IUserFavoriteRepository favoriteRepository,
        IContentItemRepository contentRepository,
        IContentSearchService searchService,
        IContentCacheService cacheService,
        IUserIntegrationService userIntegration,
        IUnitOfWork unitOfWork,
        ILogger<FavoriteCommandService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _contentRepository = contentRepository;
        _searchService = searchService;
        _cacheService = cacheService;
        _userIntegration = userIntegration;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> AddFavoriteAsync(AddFavoriteCommand command)
    {
        command.Validate();

        // Validar que el usuario sea General o Paciente (no Psicólogo)
        var userType = await _userIntegration.GetUserTypeAsync(command.UserId);
        if (userType == UserType.Psychologist)
        {
            throw new InvalidOperationException("Los psicólogos no pueden agregar favoritos");
        }

        // Verificar si ya existe
        var exists = await _favoriteRepository.ExistsAsync(command.UserId, command.ContentId);
        if (exists)
        {
            throw new InvalidOperationException("El contenido ya está en favoritos");
        }

        // Obtener el contenido del caché o buscarlo
        var content = await _contentRepository.FindByExternalIdAsync(command.ContentId);

        if (content == null)
        {
            _logger.LogWarning("Content not found in cache: {ContentId}", command.ContentId);
            throw new InvalidOperationException("Contenido no encontrado. Busca primero el contenido antes de agregarlo a favoritos.");
        }

        // Crear favorito
        var favorite = UserFavorite.Create(command.UserId, content);

        await _favoriteRepository.AddAsync(favorite);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Favorite added: {FavoriteId} for user: {UserId}",
            favorite.Id, command.UserId);

        return favorite.Id;
    }

    public async Task RemoveFavoriteAsync(RemoveFavoriteCommand command)
    {
        command.Validate();

        var favorite = await _favoriteRepository.FindByIdAsync(command.FavoriteId);

        if (favorite == null)
        {
            throw new InvalidOperationException("Favorito no encontrado");
        }

        // Validar que el favorito pertenezca al usuario
        if (!favorite.BelongsToUser(command.UserId))
        {
            throw new UnauthorizedAccessException("No tienes permiso para eliminar este favorito");
        }

        _favoriteRepository.Remove(favorite);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Favorite removed: {FavoriteId} for user: {UserId}",
            command.FavoriteId, command.UserId);
    }
}
