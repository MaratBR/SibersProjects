using System.Runtime.Serialization;

namespace SibersProjects.Services.UsersService.Exceptions;

[Serializable]
public class EmployeeException : Exception
{
    public EmployeeException()
    {
    }

    public EmployeeException(string message) : base(message)
    {
    }

    public EmployeeException(string message, Exception inner) : base(message, inner)
    {
    }

    protected EmployeeException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}