namespace BoneX.Api.Entities;

public class Appointment
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = null!; // FK to Doctor
    public string PatientId { get; set; } = null!; // FK to Patient
    public DateTime ScheduledTime { get; set; }
    public DateTime? RescheduledTime { get; set; }
    public string Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    // new properties
    public DateTime? CompletedAt { get; set; }
    public string? DiagnosisNotes { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Prescription { get; set; }
    public bool HasFollowUp { get; set; }
    public bool IsFeedbackRequested { get; set; }
    public bool IsPatientReminded { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }
    public DateTime EffectiveTime => RescheduledTime ?? ScheduledTime;
    public double? Rating { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;

    public ICollection<AppointmentFollowUp> FollowUps { get; set; } = [];
    public AppointmentFeedback? Feedback { get; set; }
}

public static class AppointmentStatus {
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

