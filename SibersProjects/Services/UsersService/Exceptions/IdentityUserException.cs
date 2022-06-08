using Microsoft.AspNetCore.Identity;

namespace SibersProjects.Services.UsersService.Exceptions;

[Serializable]
public class IdentityUserException : UserException
{
    public IdentityUserException(IEnumerable<IdentityError> errors) : base(
        "Произошла одна или более ошибок ASP.NET Identity")
    {
        Errors = errors;
    }

    public IEnumerable<IdentityError> Errors { get; }
}