using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SibersProjects.Configuration;

public class JwtSettings
{
    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string SecretKey { get; set; } = string.Empty;
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(6);

    public SecurityKey GetSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
    }
}