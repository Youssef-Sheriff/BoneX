﻿using BoneX.Api.Contracts.Patient;
using BoneX.Api.Entities;
using BoneX.Api.Errors;
using BoneX.Api.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using NetTopologySuite.Geometries;
using System.Text;

namespace BoneX.Api.Services;

public class PatientService(
    UserManager<Patient> userManager,
    ILogger<Patient> logger,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    IUserService userService) : IPatientService
{
    private readonly UserManager<Patient> _userManager = userManager;
    private readonly ILogger<Patient> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUserService _userService = userService;


    public async Task<Result> RegisterPatientAsync(PatientRegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
            return Result.Failure(UserErrors.DuplicatedEmail);

        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber,
            //ProfilePicture = request.ProfilePicture,
            PastMedicalConditions = request.PastMedicalConditions,
            ChronicConditions = request.ChronicConditions
        };

        var result = await _userManager.CreateAsync(patient, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        // Upload profile picture
        var profilePicture = await _userService.UploadProfilePictureAsync(patient.Id, request.ProfilePicture);

        if (profilePicture.IsFailure)
            return Result.Failure(ProfilePictureErrors.UploadFailed);

        patient.ProfilePicture = profilePicture.Value;

        await _userManager.UpdateAsync(patient);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(patient);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Confirmation code: {code}", code);

        // send email confirmation
        await SendConfirmationEmail(patient, code);
        return Result.Success();

        //var error = result.Errors.First();

        //return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<PatientProfileResponse>> GetPatientProfileAsync(string patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _userManager.FindByIdAsync(patientId);

        if (patient == null)
            return Result.Failure<PatientProfileResponse>(UserErrors.NotFound);

        if (patient.Role != UserRoles.Patient)
            return Result.Failure<PatientProfileResponse>(UserErrors.Unauthorized);

        var response = new PatientProfileResponse(
            Id: patient.Id,
            Email: patient.Email!,
            FirstName: patient.FirstName,
            LastName: patient.LastName,
            DateOfBirth: patient.DateOfBirth,
            Gender: patient.Gender,
            PhoneNumber: patient.PhoneNumber!,
            ProfilePicture: patient.ProfilePicture,
            Latitude: patient.Location?.X,
            Longitude: patient.Location?.Y,
            PastMedicalConditions: patient.PastMedicalConditions,
            ChronicConditions: patient.ChronicConditions
        );

        return Result.Success(response);
    }

    public async Task<Result> UpdatePatientProfileAsync(string patientId, PatientUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _userManager.FindByIdAsync(patientId);

        if (patient == null)
            return Result.Failure(UserErrors.NotFound);

        if (patient.Role != UserRoles.Patient)
            return Result.Failure(UserErrors.Unauthorized);

        // Update profile fields if provided in the request
        if (request.FirstName != null)
            patient.FirstName = request.FirstName;

        if (request.LastName != null)
            patient.LastName = request.LastName;

        if (request.DateOfBirth.HasValue)
            patient.DateOfBirth = request.DateOfBirth.Value;

        if (request.Gender.HasValue)
            patient.Gender = request.Gender.Value;

        if (request.PhoneNumber != null)
            patient.PhoneNumber = request.PhoneNumber;

        if (request.PastMedicalConditions != null)
            patient.PastMedicalConditions = request.PastMedicalConditions;

        if (request.ChronicConditions.HasValue)
            patient.ChronicConditions = request.ChronicConditions.Value;

        // Update location if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            patient.Location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };
        }

        // Handle profile picture upload if provided
        if (request.ProfilePicture != null)
        {
            var profilePicture = await _userService.UploadProfilePictureAsync(patient.Id, request.ProfilePicture);

            if (profilePicture.IsFailure)
                return Result.Failure(ProfilePictureErrors.UploadFailed);

            patient.ProfilePicture = profilePicture.Value;
        }

        // Save changes to the database
        var updateResult = await _userManager.UpdateAsync(patient);

        if (!updateResult.Succeeded)
        {
            var error = updateResult.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation", new Dictionary<string, string>
        {
            { "{{name}}", user.FirstName },
            { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
        });

        await _emailSender.SendEmailAsync(user.Email!, "✅ BoneX: Email Confirmation", emailBody);

    }
}
