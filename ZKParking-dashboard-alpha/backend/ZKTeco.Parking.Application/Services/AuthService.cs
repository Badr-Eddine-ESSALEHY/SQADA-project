using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class AuthService : IAuthService
{
    private readonly IOperatorRepository _operatorRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IOperatorRepository operatorRepository, IConfiguration configuration)
    {
        _operatorRepository = operatorRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var op = await _operatorRepository.GetByUsernameAsync(dto.Username);
        if (op == null || !op.IsActive) return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, op.PasswordHash)) return null;

        var accessToken = GenerateJwtToken(op);
        var refreshToken = GenerateRefreshToken();
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        var refreshDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        op.RefreshToken = refreshToken;
        op.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);
        op.LastLogin = DateTime.UtcNow;
        await _operatorRepository.UpdateAsync(op);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Operator = MapToDto(op)
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return null;

        var username = principal.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;

        var op = await _operatorRepository.GetByUsernameAsync(username);
        if (op == null || op.RefreshToken != refreshToken || op.RefreshTokenExpiry <= DateTime.UtcNow) return null;

        var newAccessToken = GenerateJwtToken(op);
        var newRefreshToken = GenerateRefreshToken();
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        var refreshDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        op.RefreshToken = newRefreshToken;
        op.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);
        await _operatorRepository.UpdateAsync(op);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Operator = MapToDto(op)
        };
    }

    public async Task<bool> RevokeTokenAsync(string username)
    {
        var op = await _operatorRepository.GetByUsernameAsync(username);
        if (op == null) return false;

        op.RefreshToken = null;
        op.RefreshTokenExpiry = null;
        await _operatorRepository.UpdateAsync(op);
        return true;
    }

    private string GenerateJwtToken(Domain.Entities.Operator op)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ZKTecoParkingDashboard";
        var audience = _configuration["JwtSettings:Audience"] ?? "ZKTecoParkingClients";
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, op.Username),
            new Claim(ClaimTypes.NameIdentifier, op.Id.ToString()),
            new Claim(ClaimTypes.Role, op.Role),
            new Claim("fullName", op.FullName),
            new Claim("parkingIds", op.ParkingIds)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static OperatorDto MapToDto(Domain.Entities.Operator op) => new()
    {
        Id = op.Id,
        Username = op.Username,
        FullName = op.FullName,
        Email = op.Email,
        Phone = op.Phone,
        Role = op.Role,
        ParkingIds = op.ParkingIds,
        IsActive = op.IsActive,
        LastLogin = op.LastLogin,
        CreatedAt = op.CreatedAt
    };
}
