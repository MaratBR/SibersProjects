using System.Security.Claims;

namespace SibersProjects.Utils;

public static class PrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var id = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
        {
            // TODO: кастомное исключение
            throw new InvalidOperationException("Токен не содержит информации о идентификаторе пользователя");
        }

        return id;
    }
    
    
}