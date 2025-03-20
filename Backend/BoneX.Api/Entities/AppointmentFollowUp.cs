namespace BoneX.Api.Entities;

public class AppointmentFollowUp
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string Notes { get; set; } = null!;
    public DateTime FollowUpDate { get; set; }
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;

    public Appointment Appointment { get; set; } = null!;
}

public enum FollowUpStatus
{
    Pending,
    Completed,
    Missed
}
