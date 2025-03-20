namespace BoneX.Api.Contracts.Appointments;

public record AddFeedbackRequest(
    int Rating, // 1-5
    string? Comments
);
