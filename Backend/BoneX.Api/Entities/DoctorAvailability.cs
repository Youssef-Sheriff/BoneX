namespace BoneX.Api.Entities;

public class DoctorAvailability
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Doctor Doctor { get; set; } = null!;
}
