namespace BoneX.Api.Contracts.Doctor;

public record DoctorListResponse(
    string Id,
    string FullName,
    string Speciality,
    string Brief,
    string ProfilePicture
);