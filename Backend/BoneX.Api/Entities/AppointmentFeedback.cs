namespace BoneX.Api.Entities;

public class AppointmentFeedback
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
}
