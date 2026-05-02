using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Auth;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataCache _dataCache;
    private readonly ITokenSecurity _tokenSecurity;
    private readonly IPasswordHashed _passwordHashed;

    public AuthService(IUnitOfWork unitOfWork, IDataCache dataCache, IPasswordHashed passwordHashed,
        ITokenSecurity tokenSecurity)
    {
        _unitOfWork = unitOfWork;
        _dataCache = dataCache;
        _passwordHashed = passwordHashed;
        _tokenSecurity = tokenSecurity;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        try
        {
            var repo = _unitOfWork.GetRepository<User, Guid>();

            var user = await repo.FindFirstAsync(
                u => u.Email == request.LoginIdentifier || u.Username == request.LoginIdentifier,
                tracking: false,
                cancellationToken: ct);

            if (user is null)
            {
                return Result.Failure<LoginResponse>(new Error("AuthService.LoginAsync", "User not found."));
            }

            if (!_passwordHashed.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result.Failure<LoginResponse>(new Error("AuthService.LoginAsync",
                    "Invalid username or password."));
            }

            var accessToken = _tokenSecurity.GenerateAccessToken(user);
            var refreshToken = _tokenSecurity.GenerateRefreshToken();

            await _dataCache.SetCacheAsync($"refresh_token:{user.Id}", refreshToken,
                _tokenSecurity.GetRefreshTokenExpiry());

            return Result.Success(new LoginResponse
            {
                Id = user.Id,
                RegisteredAt = user.CreatedAt,
                Role = user.Role.ToString(),
                FullName = user.FirstName + " " + user.LastName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResponse>(new Error("AuthService.LoginAsync", ex.Message));
        }
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var validation = _tokenSecurity.ValidateAccessToken(request.AccessToken, validateLifetime: false);

            if (!validation.IsValid || validation.UserId is null)
                return Result.Failure<LoginResponse>(new Error("Auth.RefreshToken", "Invalid access token."));

            if (!Guid.TryParse(validation.UserId, out var userId))
                return Result.Failure<LoginResponse>(new Error("Auth.RefreshToken", "Invalid access token."));

            var storedToken = await _dataCache.GetCacheAsync($"refresh_token:{userId}");

            if (storedToken is null || storedToken != request.RefreshToken)
                return Result.Failure<LoginResponse>(
                    new Error("Auth.RefreshToken", "Invalid or expired refresh token."));

            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.FindByIdAsync(userId, tracking: false, cancellationToken: ct);

            if (user is null)
                return Result.Failure<LoginResponse>(new Error("Auth.RefreshToken", "User not found."));

            var newAccessToken = _tokenSecurity.GenerateAccessToken(user);
            var newRefreshToken = _tokenSecurity.GenerateRefreshToken();
            var refreshExpiry = _tokenSecurity.GetRefreshTokenExpiry();

            await _dataCache.SetCacheAsync($"refresh_token:{user.Id}", newRefreshToken, refreshExpiry);
            return Result.Success(new LoginResponse
            {
                Id = user.Id,
                RegisteredAt = user.CreatedAt,
                Role = user.Role.ToString(),
                FullName = user.FirstName + " " + user.LastName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResponse>(new Error("Auth.RefreshToken", $"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result> LogoutAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var validation = _tokenSecurity.ValidateAccessToken(request.AccessToken, validateLifetime: false);

            if (!validation.IsValid || validation.UserId is null)
                return Result.Failure(new Error("Auth.Logout", "Invalid access token."));

            if (!Guid.TryParse(validation.UserId, out var userId))
                return Result.Failure(new Error("Auth.Logout", "Invalid access token."));

            var storedToken = await _dataCache.GetCacheAsync($"refresh_token:{userId}");

            if (storedToken is null || storedToken != request.RefreshToken)
                return Result.Failure(new Error("Auth.Logout", "Invalid or expired refresh token."));

            await _dataCache.DeleteCacheAsync($"refresh_token:{userId}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Auth.Logout", $"An error occurred: {ex.Message}"));
        }
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var repo = _unitOfWork.GetRepository<User, Guid>();
            
            var existingUser = await repo.AnyAsync(
                u => u.Email == request.Email || u.Username == request.Username,
                cancellationToken: ct);
            
            if (existingUser)
                return Result.Failure(new Error("AuthService.RegisterAsync", "Email or username is already taken."));
            
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = _passwordHashed.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = UserRole.Customer
            };
            
            repo.Add(user);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("AuthService.RegisterAsync", ex.Message));
        }
    }
}