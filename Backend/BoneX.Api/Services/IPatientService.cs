using BoneX.Api.Contracts.Patient;

namespace BoneX.Api.Services;

public interface IPatientService
{
    Task<Result> RegisterPatientAsync(PatientRegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<PatientProfileResponse>> GetPatientProfileAsync(string patientId, CancellationToken cancellationToken = default);
    Task<Result> UpdatePatientProfileAsync(string patientId, PatientUpdateRequest request, CancellationToken cancellationToken = default);
}
