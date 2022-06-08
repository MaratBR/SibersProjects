using System.Runtime.Serialization;

namespace SibersProjects.Services.TaskService.Exceptions;

[Serializable]
public class TaskException : Exception
{
    public TaskException()
    {
    }

    public TaskException(string message) : base(message)
    {
    }

    public TaskException(string message, Exception inner) : base(message, inner)
    {
    }

    protected TaskException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}