// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: UserQueryService
//  Estrategia: se mockean IUserRepository e ILogger y se verifica
//              el comportamiento del servicio de forma aislada.
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Users;

public class UserQueryServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IUserRepository>             _userRepoMock = new();
    private readonly Mock<ILogger<UserQueryService>>   _loggerMock   = new();

    private readonly UserQueryService _sut;

    public UserQueryServiceTests()
    {
        _sut = new UserQueryService(_userRepoMock.Object, _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static User BuildUser(
        string id       = "user-001",
        string email    = "ana@test.com",
        string fullName = "Ana García",
        UserType type   = UserType.General)
    {
        var user = new User
        {
            Email    = email,
            FullName = fullName,
            UserType = type,
            IsActive = true
        };
        SetPrivateId(user, id);
        return user;
    }

    private static void SetPrivateId(object obj, string id)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty("Id",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (prop?.CanWrite == true) { prop.SetValue(obj, id); return; }

            var field = type.GetField("<Id>k__BackingField",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            if (field != null) { field.SetValue(obj, id); return; }

            type = type.BaseType;
        }
    }

    // ─── HandleGetUserByIdAsync — Escenarios felices ──────────────────────

    [Fact]
    public async Task HandleGetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetUserByIdQuery("user-001");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("user-001"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetUserByIdAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("ana@test.com");
        result.FullName.Should().Be("Ana García");
    }

    [Fact]
    public async Task HandleGetUserByIdAsync_ExistingUser_CallsRepositoryOnce()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetUserByIdQuery("user-001");

        _userRepoMock.Setup(r => r.FindByIdAsync("user-001")).ReturnsAsync(user);

        // Act
        await _sut.HandleGetUserByIdAsync(query);

        // Assert
        _userRepoMock.Verify(r => r.FindByIdAsync("user-001"), Times.Once);
    }

    [Fact]
    public async Task HandleGetUserByIdAsync_PsychologistUser_ReturnsPsychologistType()
    {
        // Arrange
        var user  = BuildUser("psy-001", "doctor@test.com", "Dr. López", UserType.Psychologist);
        var query = new GetUserByIdQuery("psy-001");

        _userRepoMock.Setup(r => r.FindByIdAsync("psy-001")).ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetUserByIdAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.IsPsychologist().Should().BeTrue();
        result.IsGeneral().Should().BeFalse();
    }

    // ─── HandleGetUserByIdAsync — Escenarios de error ─────────────────────

    [Fact]
    public async Task HandleGetUserByIdAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var query = new GetUserByIdQuery("nonexistent-id");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.HandleGetUserByIdAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleGetUserByIdAsync_RepositoryThrows_ReturnsNull()
    {
        // Arrange
        var query = new GetUserByIdQuery("user-001");

        _userRepoMock
            .Setup(r => r.FindByIdAsync("user-001"))
            .ThrowsAsync(new Exception("Timeout"));

        // Act
        var result = await _sut.HandleGetUserByIdAsync(query);

        // Assert
        result.Should().BeNull();
    }

    // ─── HandleGetUserByEmailAsync — Escenarios felices ──────────────────

    [Fact]
    public async Task HandleGetUserByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetUserByEmailQuery("ana@test.com");

        _userRepoMock
            .Setup(r => r.FindByEmailAsync("ana@test.com"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.HandleGetUserByEmailAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("ana@test.com");
    }

    [Fact]
    public async Task HandleGetUserByEmailAsync_ExistingEmail_CallsRepositoryOnce()
    {
        // Arrange
        var user  = BuildUser();
        var query = new GetUserByEmailQuery("ana@test.com");

        _userRepoMock.Setup(r => r.FindByEmailAsync("ana@test.com")).ReturnsAsync(user);

        // Act
        await _sut.HandleGetUserByEmailAsync(query);

        // Assert
        _userRepoMock.Verify(r => r.FindByEmailAsync("ana@test.com"), Times.Once);
    }

    // ─── HandleGetUserByEmailAsync — Escenarios de error ─────────────────

    [Fact]
    public async Task HandleGetUserByEmailAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var query = new GetUserByEmailQuery("noexiste@test.com");

        _userRepoMock
            .Setup(r => r.FindByEmailAsync("noexiste@test.com"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.HandleGetUserByEmailAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleGetUserByEmailAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange — GetUserByEmailQuery.IsValid() falla si no hay '@'
        // El constructor normaliza con ToLowerInvariant pero igual valida
        var query = new GetUserByEmailQuery("not-an-email");

        // Act
        var result = await _sut.HandleGetUserByEmailAsync(query);

        // Assert
        result.Should().BeNull();
        _userRepoMock.Verify(r => r.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── HandleGetAllUsersAsync — Escenarios felices ──────────────────────

    [Fact]
    public async Task HandleGetAllUsersAsync_DefaultQuery_ReturnsUsersAndCount()
    {
        // Arrange
        var users = new List<User>
        {
            BuildUser("user-001", "ana@test.com", "Ana García"),
            BuildUser("user-002", "bob@test.com", "Bob Smith")
        };
        var query = new GetAllUsersQuery(page: 1, pageSize: 20);

        _userRepoMock
            .Setup(r => r.FindAllUsersAsync(1, 20, null, null, null, null, null, false))
            .ReturnsAsync((users, 2));

        // Act
        var (resultUsers, total) = await _sut.HandleGetAllUsersAsync(query);

        // Assert
        resultUsers.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task HandleGetAllUsersAsync_FilterByUserType_CallsRepositoryWithFilter()
    {
        // Arrange
        var users = new List<User> { BuildUser() };
        var query = new GetAllUsersQuery(userType: UserType.General);

        _userRepoMock
            .Setup(r => r.FindAllUsersAsync(1, 20, UserType.General, null, null, null, null, false))
            .ReturnsAsync((users, 1));

        // Act
        var (resultUsers, total) = await _sut.HandleGetAllUsersAsync(query);

        // Assert
        resultUsers.Should().HaveCount(1);
        total.Should().Be(1);
        _userRepoMock.Verify(r =>
            r.FindAllUsersAsync(1, 20, UserType.General, null, null, null, null, false),
            Times.Once);
    }

    [Fact]
    public async Task HandleGetAllUsersAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllUsersQuery();

        _userRepoMock
            .Setup(r => r.FindAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(),
                null, null, null, null, null, false))
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var (resultUsers, total) = await _sut.HandleGetAllUsersAsync(query);

        // Assert
        resultUsers.Should().BeEmpty();
        total.Should().Be(0);
    }

    // ─── HandleGetAllUsersAsync — Escenarios de error ────────────────────

    [Fact]
    public async Task HandleGetAllUsersAsync_RepositoryThrows_ReturnsEmptyResult()
    {
        // Arrange
        var query = new GetAllUsersQuery();

        _userRepoMock
            .Setup(r => r.FindAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(),
                null, null, null, null, null, false))
            .ThrowsAsync(new Exception("Connection lost"));

        // Act
        var (resultUsers, total) = await _sut.HandleGetAllUsersAsync(query);

        // Assert
        resultUsers.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task HandleGetAllUsersAsync_PageSizeClampedToHundred_QueryIsStillValid()
    {
        // Arrange — GetAllUsersQuery clampa PageSize a 100 si se pide más
        var query = new GetAllUsersQuery(pageSize: 999);

        // Assert — la query aún es válida con el PageSize clampeado
        query.IsValid().Should().BeTrue();
        query.PageSize.Should().Be(100);
    }
}
