namespace SibersProjects.Services.UsersService.Exceptions;

public class UserNotFoundException : UserException
{
    public UserNotFoundException(string userId) : base($"Пользователь {userId} не найден")
    {
        
    }
}