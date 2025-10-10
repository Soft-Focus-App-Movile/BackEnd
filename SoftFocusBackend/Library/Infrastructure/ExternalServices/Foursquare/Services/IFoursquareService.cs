using SoftFocusBackend.Library.Domain.Model.Aggregates;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Services;

/// <summary>
/// Servicio para interactuar con la API de Foursquare Places
/// </summary>
public interface IFoursquareService
{
    /// <summary>
    /// Busca lugares cercanos según ubicación y categorías
    /// </summary>
    Task<List<ContentItem>> SearchPlacesAsync(
        double latitude,
        double longitude,
        List<string> categoryIds,
        int radius = 5000,
        int limit = 10);

    /// <summary>
    /// Obtiene detalles de un lugar específico
    /// </summary>
    Task<ContentItem?> GetPlaceDetailsAsync(string placeId);
}
