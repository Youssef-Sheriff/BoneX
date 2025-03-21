﻿namespace BoneX.Api.Contracts.Doctor;

public record DoctorRegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string PhoneNumber,
    IFormFile ProfilePicture,
    IFormFile IdPhoto,
    double Latitude,
    double Longitude,
    string UniversityName,
    int GraduationYear,
    int YearsOfExperience,
    string ConsultationHours,
    double ConsultationFees,
    string WorkplaceName,
    IFormFile DegreeCertificate,
    IFormFile? AdditionalCertification,
    List<IFormFile>? AwardsOrRecognitions
);
