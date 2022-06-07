using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SibersProjects.Configuration;
using SibersProjects.Models;

namespace SibersProjects.Services.TokenService;

public class TokenServiceImpl : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    
    public TokenServiceImpl(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }
    
    public string GenerateUserToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            expires: DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
            claims:claims,
            signingCredentials: new SigningCredentials(
                _jwtSettings.GetSecurityKey(), 
                SecurityAlgorithms.HmacSha256));

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}