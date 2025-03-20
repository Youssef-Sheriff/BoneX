namespace BoneX.Api.Contracts.Appointments;

public record CompleteAppointmentRequest(
    string? DiagnosisNotes,
    string? TreatmentPlan,
    string? Prescription,
    bool HasFollowUp,
    bool RequestFeedback
);