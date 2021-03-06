using System.Runtime.Serialization;

namespace SibersProjects.Services.UsersService.Exceptions;

[Serializable]
public class UserException : Exception
{
    public UserException()
    {
    }

    public UserException(string message) : base(message)
    {
    }

    public UserException(string message, Exception inner) : base(message, inner)
    {
    }

    protected UserException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}