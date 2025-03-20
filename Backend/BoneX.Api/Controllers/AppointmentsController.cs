using BoneX.Api.Contracts.Appointments;

namespace BoneX.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private readonly IAppointmentService _appointmentService = appointmentService;

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        var result = await _appointmentService.CreateAppointmentAsync(request);

        return result.IsSuccess ? Ok(result) : result.ToProblem();
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetAppointmentsByDoctor(string doctorId)
    {
        var result = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("patient")]
    public async Task<IActionResult> GetAppointmentsByPatient()
    {
        var result = await _appointmentService.GetAppointmentsByPatientAsync();

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{appointmentId}/cancel")]
    public async Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentRequest request)
    {
        var result = await _appointmentService.CancelAppointmentAsync(appointmentId, request);

        return result.IsSuccess ? Ok(new { message = "Appointment canceled successfully" }) : result.ToProblem();
    }

    [HttpPut("{appointmentId}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(int appointmentId, [FromBody] RescheduleAppointmentRequest request)
    {
        var result = await _appointmentService.RescheduleAppointmentAsync(appointmentId, request);

        return result.IsSuccess ? Ok(new { message = "Appointment rescheduled successfully" }) : result.ToProblem();
    }

    [HttpPut("{appointmentId}/complete")]
    public async Task<IActionResult> CompleteAppointment(int appointmentId, [FromBody] CompleteAppointmentRequest request)
    {
        var result = await _appointmentService.CompleteAppointmentAsync(appointmentId, request);

        return result.IsSuccess
            ? Ok(new { message = "Appointment completed successfully" })
            : result.ToProblem();
    }

    [HttpPost("{appointmentId}/followup")]
    public async Task<IActionResult> CreateFollowUp(int appointmentId, [FromBody] CreateFollowUpRequest request)
    {
        var result = await _appointmentService.CreateFollowUpAsync(appointmentId, request);
        return result.IsSuccess
            ? Ok(new { message = "Follow-up created successfully" })
            : result.ToProblem();
    }

    [HttpGet("stats/doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorAppointmentStats(string doctorId)
    {
        var result = await _appointmentService.GetDoctorAppointmentStatsAsync(doctorId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{appointmentId}/feedback")]
    public async Task<IActionResult> AddAppointmentFeedback(int appointmentId, [FromBody] AddFeedbackRequest request)
    {
        var result = await _appointmentService.AddAppointmentFeedbackAsync(appointmentId, request);
        return result.IsSuccess
            ? Ok(new { message = "Feedback submitted successfully" })
            : result.ToProblem();
    }
}
