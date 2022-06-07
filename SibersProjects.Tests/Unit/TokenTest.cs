using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Configuration;
using SibersProjects.Models;
using SibersProjects.Services.TokenService;

namespace SibersProjects.Tests.Unit;

public class TokenTest : BaseTest
{
    [Fact]
    public async Task TestUserTokenGeneration()
    {
        var settings = ServiceProvider.GetRequiredService<JwtSettings>();
        var tokenService = ServiceProvider.GetRequiredService<ITokenService>();
        var user = new User { UserName = "admin", Email = "admin@test.net" };
        var tokenStr = await tokenService.GenerateUserToken(user);
        Assert.Equal(3, tokenStr.Split('.').Length);
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenStr);
        Assert.Equal(jwtToken.Issuer, settings.Issuer);
        Assert.Equal(jwtToken.Subject, user.Id);
        Assert.Equal("admin", jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value);
        Assert.Equal("admin@test.net", jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
    }
}