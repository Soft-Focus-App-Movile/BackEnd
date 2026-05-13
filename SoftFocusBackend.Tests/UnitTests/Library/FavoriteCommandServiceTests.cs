// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: FavoriteCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tests.UnitTests.Library;

public class FavoriteCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserFavoriteRepository>       _favoriteRepoMock  = new();
    private readonly Mock<IContentItemRepository>        _contentRepoMock   = new();
    private readonly Mock<IContentSearchService>         _searchServiceMock = new();
    private readonly Mock<IContentCacheService>          _cacheServiceMock  = new();
    private readonly Mock<IUserIntegrationService>       _userIntMock       = new();
    private readonly Mock<IUnitOfWork>                   _unitOfWorkMock    = new();
    private readonly Mock<ILogger<FavoriteCommandService>> _loggerMock      = new();

    private readonly FavoriteCommandService _sut;

    public FavoriteCommandServiceTests()
    {
        _sut = new FavoriteCommandService(
            _favoriteRepoMock.Object,
            _contentRepoMock.Object,
            _searchServiceMock.Object,
            _cacheServiceMock.Object,
            _userIntMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ContentItem BuildContentItem(string externalId = "tmdb-movie-001")
    {
        var metadata = new ContentMetadata { Title = "Película Test" };
        return ContentItem.Create(externalId, ContentType.Movie, metadata, new List<EmotionalTag> { EmotionalTag.Happy }, "https://test.com", 24);
    }

    private static AddFavoriteCommand BuildAddCommand(
        string userId    = "user-001",
        string contentId = "tmdb-movie-001") =>
        new(userId, contentId, ContentType.Movie);

    private static RemoveFavoriteCommand BuildRemoveCommand(
        string userId     = "user-001",
        string favoriteId = "fav-001") =>
        new(userId, favoriteId);

    // ─── AddFavoriteAsync — Escenarios felices ────────────────────────────

    [Fact]
    public async Task AddFavoriteAsync_GeneralUser_WithCachedContent_ReturnsId()
    {
        // Arrange
        var command = BuildAddCommand();
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.General);
        _favoriteRepoMock.Setup(r => r.ExistsAsync(command.UserId, command.ContentId)).ReturnsAsync(false);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync(command.ContentId)).ReturnsAsync(content);
        _favoriteRepoMock.Setup(r => r.AddAsync(It.IsAny<UserFavorite>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddFavoriteAsync(command);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AddFavoriteAsync_ValidUser_CallsRepositoryAddOnce()
    {
        // Arrange
        var command = BuildAddCommand();
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.General);
        _favoriteRepoMock.Setup(r => r.ExistsAsync(command.UserId, command.ContentId)).ReturnsAsync(false);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync(command.ContentId)).ReturnsAsync(content);
        _favoriteRepoMock.Setup(r => r.AddAsync(It.IsAny<UserFavorite>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.AddFavoriteAsync(command);

        // Assert
        _favoriteRepoMock.Verify(r => r.AddAsync(It.IsAny<UserFavorite>()), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_ValidUser_CompletesUnitOfWork()
    {
        // Arrange
        var command = BuildAddCommand();
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.General);
        _favoriteRepoMock.Setup(r => r.ExistsAsync(command.UserId, command.ContentId)).ReturnsAsync(false);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync(command.ContentId)).ReturnsAsync(content);
        _favoriteRepoMock.Setup(r => r.AddAsync(It.IsAny<UserFavorite>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.AddFavoriteAsync(command);

        // Assert
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // ─── AddFavoriteAsync — Escenarios de error ───────────────────────────

    [Fact]
    public async Task AddFavoriteAsync_PsychologistUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildAddCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.Psychologist);

        // Act
        var act = async () => await _sut.AddFavoriteAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*psicólogos*");
        _favoriteRepoMock.Verify(r => r.AddAsync(It.IsAny<UserFavorite>()), Times.Never);
    }

    [Fact]
    public async Task AddFavoriteAsync_ContentAlreadyInFavorites_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildAddCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.General);
        _favoriteRepoMock.Setup(r => r.ExistsAsync(command.UserId, command.ContentId)).ReturnsAsync(true);

        // Act
        var act = async () => await _sut.AddFavoriteAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*favoritos*");
        _favoriteRepoMock.Verify(r => r.AddAsync(It.IsAny<UserFavorite>()), Times.Never);
    }

    [Fact]
    public async Task AddFavoriteAsync_ContentNotFoundInCache_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildAddCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync(command.UserId)).ReturnsAsync(UserType.General);
        _favoriteRepoMock.Setup(r => r.ExistsAsync(command.UserId, command.ContentId)).ReturnsAsync(false);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync(command.ContentId)).ReturnsAsync((ContentItem?)null);

        // Act
        var act = async () => await _sut.AddFavoriteAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrado*");
        _favoriteRepoMock.Verify(r => r.AddAsync(It.IsAny<UserFavorite>()), Times.Never);
    }

    // ─── RemoveFavoriteAsync — Escenarios felices ─────────────────────────

    [Fact]
    public async Task RemoveFavoriteAsync_ValidOwner_RemovesFavorite()
    {
        // Arrange
        var content  = BuildContentItem();
        var favorite = UserFavorite.Create("user-001", content);
        var command  = BuildRemoveCommand("user-001", "fav-001");

        _favoriteRepoMock.Setup(r => r.FindByIdAsync("fav-001")).ReturnsAsync(favorite);
        _favoriteRepoMock.Setup(r => r.Remove(favorite));
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.RemoveFavoriteAsync(command);

        // Assert
        _favoriteRepoMock.Verify(r => r.Remove(favorite), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ValidOwner_CompletesUnitOfWork()
    {
        // Arrange
        var content  = BuildContentItem();
        var favorite = UserFavorite.Create("user-001", content);
        var command  = BuildRemoveCommand("user-001", "fav-001");

        _favoriteRepoMock.Setup(r => r.FindByIdAsync("fav-001")).ReturnsAsync(favorite);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.RemoveFavoriteAsync(command);

        // Assert
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // ─── RemoveFavoriteAsync — Escenarios de error ────────────────────────

    [Fact]
    public async Task RemoveFavoriteAsync_FavoriteNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildRemoveCommand("user-001", "nonexistent-fav");

        _favoriteRepoMock
            .Setup(r => r.FindByIdAsync("nonexistent-fav"))
            .ReturnsAsync((UserFavorite?)null);

        // Act
        var act = async () => await _sut.RemoveFavoriteAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrado*");
        _favoriteRepoMock.Verify(r => r.Remove(It.IsAny<UserFavorite>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WrongUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange — el favorito pertenece a "other-user", no a "user-001"
        var content  = BuildContentItem();
        var favorite = UserFavorite.Create("other-user", content);
        var command  = BuildRemoveCommand("user-001", "fav-001");

        _favoriteRepoMock.Setup(r => r.FindByIdAsync("fav-001")).ReturnsAsync(favorite);

        // Act
        var act = async () => await _sut.RemoveFavoriteAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _favoriteRepoMock.Verify(r => r.Remove(It.IsAny<UserFavorite>()), Times.Never);
    }
}
