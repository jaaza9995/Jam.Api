using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Jam.Api.Models;

namespace Jam.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<AuthUser> _userManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration config, UserManager<AuthUser> userManager, ILogger<TokenService> logger)
    {
        _config = config;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> GenerateJwtTokenAsync(AuthUser user)
    {
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            _logger.LogError("JWT key is missing from configuration.");
            throw new InvalidOperationException("JWT key is missing from configuration.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            // Use plain "role" claim so frontend sees "role": "Admin" (or array)
            claims.Add(new Claim("role", role));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials);

        _logger.LogInformation("JWT token created for {Username}", user.UserName);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}