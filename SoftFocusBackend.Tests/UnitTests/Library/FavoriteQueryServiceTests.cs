// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: FavoriteQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Library;

public class FavoriteQueryServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserFavoriteRepository>         _favoriteRepoMock = new();
    private readonly Mock<ILogger<FavoriteQueryService>>   _loggerMock       = new();

    private readonly FavoriteQueryService _sut;

    public FavoriteQueryServiceTests()
    {
        _sut = new FavoriteQueryService(
            _favoriteRepoMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ContentItem BuildContentItem(EmotionalTag emotionalTag = EmotionalTag.Happy)
    {
        var metadata = new ContentMetadata { Title = "Contenido Test" };
        return ContentItem.Create("ext-001", ContentType.Movie, metadata, new List<EmotionalTag> { emotionalTag }, "https://test.com", 24);
    }

    private static UserFavorite BuildFavorite(string userId = "user-001")
    {
        var content = BuildContentItem(EmotionalTag.Calm);
        return UserFavorite.Create(userId, content);
    }

    // ─── GetFavoritesAsync — Escenarios felices ───────────────────────────

    [Fact]
    public async Task GetFavoritesAsync_NoFilter_ReturnsAllFavorites()
    {
        // Arrange
        var favorites = new List<UserFavorite> { BuildFavorite(), BuildFavorite() };
        var query     = new GetFavoritesQuery("user-001");

        _favoriteRepoMock
            .Setup(r => r.FindByUserIdAsync("user-001"))
            .ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync(query);

        // Assert
        result.Should().HaveCount(2);
        _favoriteRepoMock.Verify(r => r.FindByUserIdAsync("user-001"), Times.Once);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithContentTypeFilter_ReturnsFilteredByType()
    {
        // Arrange
        var favorites = new List<UserFavorite> { BuildFavorite() };
        var query     = new GetFavoritesQuery("user-001", contentTypeFilter: ContentType.Movie);

        _favoriteRepoMock
            .Setup(r => r.FindByUserIdAndTypeAsync("user-001", ContentType.Movie))
            .ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync(query);

        // Assert
        result.Should().HaveCount(1);
        _favoriteRepoMock.Verify(r => r.FindByUserIdAndTypeAsync("user-001", ContentType.Movie), Times.Once);
        _favoriteRepoMock.Verify(r => r.FindByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithEmotionFilter_ReturnsFilteredByEmotion()
    {
        // Arrange
        var favorites = new List<UserFavorite> { BuildFavorite() };
        var query     = new GetFavoritesQuery("user-001", emotionFilter: EmotionalTag.Calm);

        _favoriteRepoMock
            .Setup(r => r.FindByUserIdAndEmotionAsync("user-001", EmotionalTag.Calm))
            .ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync(query);

        // Assert
        result.Should().HaveCount(1);
        _favoriteRepoMock.Verify(r => r.FindByUserIdAndEmotionAsync("user-001", EmotionalTag.Calm), Times.Once);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithBothFilters_FiltersResultsByEmotion()
    {
        // Arrange — primero filtra por tipo, luego filtra en memoria por emoción
        var contentWithCalm   = BuildContentItem(EmotionalTag.Calm);
        var contentWithHappy  = BuildContentItem(EmotionalTag.Happy);
        var favWithCalm       = UserFavorite.Create("user-001", contentWithCalm);
        var favWithHappy      = UserFavorite.Create("user-001", contentWithHappy);
        var favorites         = new List<UserFavorite> { favWithCalm, favWithHappy };

        var query = new GetFavoritesQuery("user-001", contentTypeFilter: ContentType.Movie, emotionFilter: EmotionalTag.Calm);

        _favoriteRepoMock
            .Setup(r => r.FindByUserIdAndTypeAsync("user-001", ContentType.Movie))
            .ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync(query);

        // Assert — solo el favorito con Calm debe pasar el filtro
        result.Should().HaveCount(1);
        result.First().Content.EmotionalTags.Should().Contain(EmotionalTag.Calm);
    }

    [Fact]
    public async Task GetFavoritesAsync_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetFavoritesQuery(string.Empty);

        // Act
        var act = async () => await _sut.GetFavoritesAsync(query);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetFavoritesAsync_NoFavorites_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetFavoritesQuery("user-001");

        _favoriteRepoMock
            .Setup(r => r.FindByUserIdAsync("user-001"))
            .ReturnsAsync(new List<UserFavorite>());

        // Act
        var result = await _sut.GetFavoritesAsync(query);

        // Assert
        result.Should().BeEmpty();
    }
}
