using BoneX.Api.Contracts.Appointments;

namespace BoneX.Api.Services;

public interface IAppointmentService
{
    Task<Result> CreateAppointmentAsync(CreateAppointmentRequest request);
    Task<Result<List<AppointmentResponse>>> GetAppointmentsByDoctorAsync(string doctorId);
    Task<Result<List<AppointmentResponse>>> GetAppointmentsByPatientAsync();
    Task<Result> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequest request);
    Task<Result> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequest request);
    Task<Result> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentRequest request);
    Task<Result> CreateFollowUpAsync(int appointmentId, CreateFollowUpRequest request);
    Task<Result<DoctorAppointmentStats>> GetDoctorAppointmentStatsAsync(string doctorId);
    Task<Result> AddAppointmentFeedbackAsync(int appointmentId, AddFeedbackRequest request);
}
