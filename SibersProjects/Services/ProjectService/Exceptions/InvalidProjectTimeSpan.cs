namespace SibersProjects.Services.ProjectService.Exceptions;

public class InvalidProjectTimeSpan : ProjectException
{
    public InvalidProjectTimeSpan(DateTime startsAt, DateTime endsAt, string message)
        : base($"Неверные значения даты начала/конца: {message}")
    {
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public DateTime StartsAt { get; }
    public DateTime EndsAt { get; }
}