using System.Runtime.Serialization;

namespace SibersProjects.Services.ProjectService.Exceptions;

[Serializable]
public class ProjectException : Exception
{
    public ProjectException()
    {
    }

    public ProjectException(string message) : base(message)
    {
    }

    public ProjectException(string message, Exception inner) : base(message, inner)
    {
    }

    protected ProjectException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}