namespace SibersProjects.Services.UsersService.Exceptions;

[Serializable]
public class CurrentUserMissingException : UserException
{
    public CurrentUserMissingException() : base("Текущий пользователь отсутсвует в базе данных")
    {
    }
}