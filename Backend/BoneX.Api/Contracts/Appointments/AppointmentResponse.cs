namespace BoneX.Api.Contracts.Appointments;

public record AppointmentResponse(
    int Id,
    string DoctorId,
    string PatientId,
    DateTime Date,
    string? Reason,
    string Status
);