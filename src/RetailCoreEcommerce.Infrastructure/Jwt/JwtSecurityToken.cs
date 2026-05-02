using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Models.Token;
using RetailCoreEcommerce.Contracts.Settings;
using RetailCoreEcommerce.Contracts.Shared;
using RetailCoreEcommerce.Domain;
using TokenValidationResult = RetailCoreEcommerce.Contracts.Models.Token.TokenValidationResult;

namespace RetailCoreEcommerce.Infrastructure.Jwt;

public class JwtSecurityToken : ITokenSecurity
{
    private readonly JwtSettings _jwtSettings;
    private readonly RSA _privateRsa;
    private readonly RSA _publicRsa;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtSecurityToken(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        if (string.IsNullOrEmpty(_jwtSettings.PrivateKeyBytes))
            throw new Exception("Private key is not configured. Please check your JwtSettings.");

        if (string.IsNullOrEmpty(_jwtSettings.PublicKeyBytes))
            throw new Exception("Public key is not configured. Please check your JwtSettings.");

        _privateRsa = _jwtSettings.PrivateKeyBytes.ReadRsaKeyBase64();
        _publicRsa = _jwtSettings.PublicKeyBytes.ReadRsaKeyBase64();
    }

    // ── Public interface methods 

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        return GenerateToken(claims);
    }

    public TokenValidationResult ValidateAccessToken(string token, bool validateLifetime = true)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(
                token, GetValidationParameters(validateLifetime), out _);

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)!.ToString() ??
                         throw new Exception("Token does not contain 'sub' claim");

            var claims = principal.Claims
                .Select(c => new TokenClaim(c.Type, c.Value))
                .ToList()
                .AsReadOnly();

            return new TokenValidationResult(IsValid: true, UserId: userId, Claims: claims);
        }
        catch
        {
            return new TokenValidationResult(IsValid: false, UserId: null, Claims: []);
        }
    }

    public DateTime? GetExpirationTimeFromToken(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var expClaim = jwtToken.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            if (expClaim is null) return null;

            var unixTime = long.Parse(expClaim.Value);
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public TimeSpan GetRefreshTokenExpiry()
        => TimeSpan.FromMinutes(_jwtSettings.RefreshTokenExpirationMinutes);

    // ── Private helpers ─────────────────────────────────────────────────────

    private string GenerateToken(IEnumerable<Claim> claims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new RsaSecurityKey(_privateRsa), SecurityAlgorithms.RsaSha256),
        };

        var token = _tokenHandler.CreateToken(descriptor);
        return _tokenHandler.WriteToken(token);
    }

    private TokenValidationParameters GetValidationParameters(bool validateLifetime) =>
        new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(_publicRsa),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero,
        };
}