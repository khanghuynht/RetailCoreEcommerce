using System.Linq.Expressions;
using Moq;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Application.Services;
using RetailCoreEcommerce.Contracts.Infrastructure;
using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.Contracts.Models.Token;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;
using Xunit;

namespace RetailCoreEcommerce.Test.Auth;

public class AuthServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly TimeSpan RefreshExpiry = TimeSpan.FromDays(7);

    private static readonly User SampleUser = new()
    {
        Id = UserId,
        Email = "u@test.com",
        Username = "user1",
        PasswordHash = "hash",
        FirstName = "Jane",
        LastName = "Doe",
        Role = UserRole.Customer,
        CreatedAt = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc)
    };

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.FindFirstAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut(repo);

        // Act
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "missing@test.com", Password = "x" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.FindFirstAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(SampleUser);

        var password = new Mock<IPasswordHashed>();
        password.Setup(p => p.VerifyPassword("wrong", SampleUser.PasswordHash)).Returns(false);

        var sut = CreateSut(repo, passwordHashed: password);

        // Act
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = SampleUser.Email, Password = "wrong" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_CachesRefreshTokenAndReturnsTokens()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.FindFirstAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(SampleUser);

        var password = new Mock<IPasswordHashed>();
        password.Setup(p => p.VerifyPassword("secret", SampleUser.PasswordHash)).Returns(true);

        var tokens = new Mock<ITokenSecurity>();

        var userClaim = new UserClaim(
            SampleUser.Id.ToString(),
            SampleUser.Email,
            SampleUser.Username,
            SampleUser.FirstName,
            SampleUser.LastName,
            SampleUser.Role.ToString());

        tokens.Setup(t => t.GenerateAccessToken(userClaim)).Returns("access-jwt");
        tokens.Setup(t => t.GenerateRefreshToken()).Returns("refresh-xyz");
        tokens.Setup(t => t.GetRefreshTokenExpiry()).Returns(RefreshExpiry);

        var cache = new Mock<IDataCache>();

        var sut = CreateSut(repo, cache, password, tokens);

        // Act
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = SampleUser.Email, Password = "secret" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-jwt", result.Value!.AccessToken);
        Assert.Equal("refresh-xyz", result.Value.RefreshToken);
        Assert.Equal($"{SampleUser.FirstName} {SampleUser.LastName}", result.Value.FullName);
        Assert.Equal(SampleUser.Role.ToString(), result.Value.Role);

        cache.Verify(c => c.SetCacheAsync($"refresh_token:{UserId}", "refresh-xyz", RefreshExpiry, true),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidAccessToken_ReturnsFailure()
    {
        // Arrange
        var tokens = new Mock<ITokenSecurity>();
        tokens.Setup(t => t.ValidateAccessToken("bad", false))
            .Returns(new TokenValidationResult(false, null, Array.Empty<TokenClaim>()));

        var sut = CreateSut(tokenSecurity: tokens);

        // Act
        var result = await sut.RefreshTokenAsync(
            new RefreshTokenRequest { AccessToken = "bad", RefreshToken = "r" }, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid access token", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshTokenAsync_StoredRefreshMismatch_ReturnsFailure()
    {
        // Arrange
        var tokens = new Mock<ITokenSecurity>();
        tokens.Setup(t => t.ValidateAccessToken("access", false))
            .Returns(new TokenValidationResult(true, UserId.ToString(), Array.Empty<TokenClaim>()));

        var cache = new Mock<IDataCache>();
        cache.Setup(c => c.GetCacheAsync($"refresh_token:{UserId}")).ReturnsAsync("other-refresh");

        var sut = CreateSut(cacheMock: cache, tokenSecurity: tokens);

        // Act
        var result = await sut.RefreshTokenAsync(
            new RefreshTokenRequest { AccessToken = "access", RefreshToken = "sent-refresh" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid or expired refresh token", result.Error!.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidRequest_ReturnsNewTokensAndUpdatesCache()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.FindByIdAsync(UserId, false, It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(SampleUser);

        var tokens = new Mock<ITokenSecurity>();
        tokens.Setup(t => t.ValidateAccessToken("access", false))
            .Returns(new TokenValidationResult(true, UserId.ToString(), Array.Empty<TokenClaim>()));
        
        var userClaim = new UserClaim(
            SampleUser.Id.ToString(),
            SampleUser.Email,
            SampleUser.Username,
            SampleUser.FirstName,
            SampleUser.LastName,
            SampleUser.Role.ToString());
        
        tokens.Setup(t => t.GenerateAccessToken(userClaim)).Returns("new-access");
        tokens.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");
        tokens.Setup(t => t.GetRefreshTokenExpiry()).Returns(RefreshExpiry);

        var cache = new Mock<IDataCache>();
        cache.Setup(c => c.GetCacheAsync($"refresh_token:{UserId}")).ReturnsAsync("old-refresh");

        var sut = CreateSut(repo, cache, tokenSecurity: tokens);

        // Act
        var result = await sut.RefreshTokenAsync(
            new RefreshTokenRequest { AccessToken = "access", RefreshToken = "old-refresh" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-access", result.Value!.AccessToken);
        Assert.Equal("new-refresh", result.Value.RefreshToken);
        cache.Verify(c => c.SetCacheAsync($"refresh_token:{UserId}", "new-refresh", RefreshExpiry, true),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ValidRequest_DeletesRefreshCache()
    {
        // Arrange
        var tokens = new Mock<ITokenSecurity>();
        tokens.Setup(t => t.ValidateAccessToken("access", false))
            .Returns(new TokenValidationResult(true, UserId.ToString(), Array.Empty<TokenClaim>()));

        var cache = new Mock<IDataCache>();
        cache.Setup(c => c.GetCacheAsync($"refresh_token:{UserId}")).ReturnsAsync("same-refresh");

        var sut = CreateSut(cacheMock: cache, tokenSecurity: tokens);

        // Act
        var result = await sut.LogoutAsync(
            new RefreshTokenRequest { AccessToken = "access", RefreshToken = "same-refresh" },
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        cache.Verify(c => c.DeleteCacheAsync($"refresh_token:{UserId}"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailTaken_ReturnsFailure()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(true);

        var sut = CreateSut(repo);

        var request = new RegisterRequest
        {
            Email = "taken@test.com",
            Username = "newuser",
            Password = "p",
            PhoneNumber = "1",
            FirstName = "A",
            LastName = "B"
        };

        // Act
        var result = await sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("already taken", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
        repo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_AddsAndSaves()
    {
        // Arrange
        var repo = new Mock<IGenericRepository<User, Guid>>();
        repo.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(false);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.GetRepository<User, Guid>()).Returns(repo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(1);

        var password = new Mock<IPasswordHashed>();
        password.Setup(p => p.HashPassword("secret")).Returns("hashed-secret");

        var sut = new AuthService(uow.Object, Mock.Of<IDataCache>(), password.Object, Mock.Of<ITokenSecurity>());

        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Username = "newbie",
            Password = "secret",
            PhoneNumber = "+1",
            FirstName = "N",
            LastName = "U"
        };

        // Act
        var result = await sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        repo.Verify(r => r.Add(It.Is<User>(u =>
            u.Email == request.Email &&
            u.Username == request.Username &&
            u.PasswordHash == "hashed-secret" &&
            u.Role == UserRole.Customer)), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(true, true), Times.Once);
    }

    private static AuthService CreateSut(
        Mock<IGenericRepository<User, Guid>> userRepo,
        Mock<IDataCache>? cacheMock = null,
        Mock<IPasswordHashed>? passwordHashed = null,
        Mock<ITokenSecurity>? tokenSecurity = null)
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.GetRepository<User, Guid>()).Returns(userRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(1);

        return new AuthService(
            uow.Object,
            cacheMock?.Object ?? Mock.Of<IDataCache>(),
            passwordHashed?.Object ?? Mock.Of<IPasswordHashed>(),
            tokenSecurity?.Object ?? Mock.Of<ITokenSecurity>());
    }

    private static AuthService CreateSut(
        Mock<IDataCache>? cacheMock = null,
        Mock<ITokenSecurity>? tokenSecurity = null)
    {
        var userRepo = new Mock<IGenericRepository<User, Guid>>();
        return CreateSut(userRepo, cacheMock, tokenSecurity: tokenSecurity);
    }
}