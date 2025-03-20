namespace BoneX.Api.Contracts.Appointments;

public record CreateFollowUpRequest(
    DateTime FollowUpDate,
    string Notes
);