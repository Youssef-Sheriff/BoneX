namespace BoneX.Api.Contracts.Appointments;

public record RescheduleAppointmentRequest(
    DateTime NewTime
);