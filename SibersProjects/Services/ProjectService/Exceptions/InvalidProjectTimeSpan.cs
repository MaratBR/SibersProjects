namespace SibersProjects.Services.ProjectService.Exceptions;

public class InvalidProjectTimeSpan : ProjectException
{
    public DateTime StartsAt { get; }
    public DateTime EndsAt { get; }

    
    public InvalidProjectTimeSpan(DateTime startsAt, DateTime endsAt, string message)
        : base($"Неверные значения даты начала/конца: {message}")
    {
        StartsAt = startsAt;
        EndsAt = endsAt;
    }
}