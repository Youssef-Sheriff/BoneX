namespace BoneX.Api.Contracts.Doctor;

public record UpdateDoctorProfileRequest(
    //string? FirstName,
    //string? LastName,
    string? PhoneNumber,
    IFormFile? ProfilePicture,
    //string? UniversityName,
    //int? GraduationYear,
    int? YearsOfExperience,
    string? ConsultationHours,
    double? ConsultationFees,
    string? WorkplaceName,
    string? Brief,
    string? Award,
    IFormFile? AwardImage,
    //IFormFile? DegreeCertificate,
    IFormFile? AdditionalCertification,
    List<IFormFile>? AwardsOrRecognitions,
    double? Latitude,
    double? Longitude
);
