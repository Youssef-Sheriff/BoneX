using BoneX.Api.Contracts.Appointments;
using BoneX.Api.Errors;
using BoneX.Api.Extensions;

namespace BoneX.Api.Services;

public class AppointmentService(ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AppointmentService> logger) : IAppointmentService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<AppointmentService> _logger = logger;

    public async Task<Result> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        var patientId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(patientId))
            return Result.Failure(AppointmentErrors.PatientNotAuthenticated);

        var patient = await _context.Patients.FindAsync(patientId);

        if (patient is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        if (patient.Role != UserRoles.Patient)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);


        // Check for rate limiting
        var result = await EnforceAppointmentLimitsAsync(patientId);

        if (!result.IsSuccess)
            return result;

        var doctor = await _context.Doctors.FindAsync(request.DoctorId);

        if (doctor is null)
            return Result.Failure(AppointmentErrors.DoctorNotFound);

        //var isAvailable = await IsDoctorAvailable(request.DoctorId, request.ScheduledTime);

        //if (!isAvailable)
        //    return Result.Failure(AppointmentErrors.AppointmentTimeConflict);

        var appointment = new Appointment
        {
            DoctorId = request.DoctorId,
            PatientId = patientId,
            ScheduledTime = request.ScheduledTime,
            Status = AppointmentStatus.Scheduled,
            Notes = request.Notes
        };

        await _context.AddAsync(appointment);
        await _context.SaveChangesAsync();

        return Result.Success(appointment.Id);
    }

    public async Task<Result<List<AppointmentResponse>>> GetAppointmentsByDoctorAsync(string doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);

        if (doctor is null)
            return Result.Failure<List<AppointmentResponse>>(AppointmentErrors.DoctorNotFound);

        if (doctor.Role != UserRoles.Doctor)
            return Result.Failure<List<AppointmentResponse>>(UserErrors.Unauthorized);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId)
            .Select(x => new AppointmentResponse
            (
                x.Id,
                x.DoctorId,
                x.PatientId,
                x.ScheduledTime,
                x.Notes,
                x.Status
            ))
            .ToListAsync();

        return Result.Success(appointments);
    }

    public async Task<Result<List<AppointmentResponse>>> GetAppointmentsByPatientAsync()
    {
        var patientId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(patientId))
            return Result.Failure<List<AppointmentResponse>>(AppointmentErrors.PatientNotAuthenticated);

        var patient = await _context.Patients.FindAsync(patientId);

        if (patient is null)
            return Result.Failure<List<AppointmentResponse>>(UserErrors.InvalidJwtToken);

        if (patient.Role != UserRoles.Patient)
            return Result.Failure<List<AppointmentResponse>>(UserErrors.Unauthorized);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .Include(x => x.Doctor)
            .Select(x => new AppointmentResponse
            (
                x.Id,
                x.DoctorId,
                x.PatientId,
                x.ScheduledTime,
                x.Notes,
                x.Status
            ))
            .ToListAsync();

        return Result.Success(appointments);
    }

    public async Task<Result> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(AppointmentErrors.PatientNotAuthenticated);

        var appointment = await _context.Appointments.FindAsync(appointmentId);

        if (appointment is null)
            return Result.Failure(AppointmentErrors.AppointmentNotFound);

        if (appointment.PatientId != userId && appointment.DoctorId != userId)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason;
        appointment.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(AppointmentErrors.PatientNotAuthenticated);

        var appointment = await _context.Appointments.FindAsync(appointmentId);

        if (appointment is null)
            return Result.Failure(AppointmentErrors.AppointmentNotFound);

        if (appointment.PatientId != userId && appointment.DoctorId != userId)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result.Failure(AppointmentErrors.AppointmentAlreadyCancelled);

        if (appointment.Status == AppointmentStatus.Completed)
            return Result.Failure(AppointmentErrors.AppointmentAlreadyCompleted);

        //// Check doctor's availability
        //var isAvailable = await IsDoctorAvailable(appointment.DoctorId, request.NewTime);

        //if (!isAvailable)
        //    return Result.Failure(AppointmentErrors.AppointmentTimeConflict);

        // Update appointment time
        if (appointment.RescheduledTime is null)
        {
            appointment.RescheduledTime = request.NewTime;
        }
        else
        {
            // If already rescheduled once, update the ScheduledTime instead
            appointment.ScheduledTime = appointment.RescheduledTime.Value;
            appointment.RescheduledTime = request.NewTime;
        }

        appointment.Status = AppointmentStatus.Scheduled;
        appointment.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> CreateFollowUpAsync(int appointmentId, CreateFollowUpRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return Result.Failure(AppointmentErrors.AppointmentNotFound);

        if (appointment.DoctorId != userId)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        // Only allow follow-ups for completed appointments
        if (appointment.Status != AppointmentStatus.Completed)
            return Result.Failure(AppointmentErrors.AppointmentNotCompleted);

        var followUp = new AppointmentFollowUp
        {
            AppointmentId = appointmentId,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate
        };

        await _context.AddAsync(followUp);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<DoctorAppointmentStats>> GetDoctorAppointmentStatsAsync(string doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);

        if (doctor == null)
            return Result.Failure<DoctorAppointmentStats>(AppointmentErrors.DoctorNotFound);

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var stats = new DoctorAppointmentStats
        {
            TotalAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId),

            CompletedAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.Status == AppointmentStatus.Completed),

            CancelledAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.Status == AppointmentStatus.Cancelled),

            TodayAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                 a.ScheduledTime.Date == today),

            ThisWeekAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.ScheduledTime >= startOfWeek &&
                                a.ScheduledTime < endOfWeek)
        };

        return Result.Success(stats);
    }

    public async Task<Result> AddAppointmentFeedbackAsync(int appointmentId, AddFeedbackRequest request)
    {
        var patientId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(patientId))
            return Result.Failure(AppointmentErrors.PatientNotAuthenticated);

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return Result.Failure(AppointmentErrors.AppointmentNotFound);

        if (appointment.PatientId != patientId)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        if (appointment.Status != AppointmentStatus.Completed)
            return Result.Failure(AppointmentErrors.AppointmentNotCompleted);

        // Check if feedback already exists
        var existingFeedback = await _context.AppointmentFeedbacks
            .AnyAsync(f => f.AppointmentId == appointmentId);

        if (existingFeedback)
            return Result.Failure(AppointmentErrors.FeedbackAlreadySubmitted);

        // Validate rating
        if (request.Rating < 1 || request.Rating > 5)
            return Result.Failure(new Error(
                "Appointment.InvalidRating",
                "Rating must be between 1 and 5",
                StatusCodes.Status400BadRequest));

        var feedback = new AppointmentFeedback
        {
            AppointmentId = appointmentId,
            Rating = request.Rating,
            Comments = request.Comments
        };

        await _context.AddAsync(feedback);

        appointment.Rating = request.Rating;

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        var appointment = await _context.Appointments.FindAsync(appointmentId);

        if (appointment == null)
            return Result.Failure(AppointmentErrors.AppointmentNotFound);

        if (appointment.DoctorId != userId)
            return Result.Failure(AppointmentErrors.UnauthorizedAccess);

        if (appointment.Status != AppointmentStatus.Scheduled)
            return Result.Failure(AppointmentErrors.AppointmentNotScheduled);

        if (appointment.Status == AppointmentStatus.Completed)
            return Result.Failure(AppointmentErrors.AppointmentAlreadyCompleted);

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.DiagnosisNotes = request.DiagnosisNotes;
        appointment.TreatmentPlan = request.TreatmentPlan;
        appointment.Prescription = request.Prescription;
        appointment.HasFollowUp = request.HasFollowUp;
        appointment.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Request feedback from patient
        if (request.RequestFeedback)
        {
            appointment.IsFeedbackRequested = true;
            await _context.SaveChangesAsync();
        }

        return Result.Success();
    }

    private async Task<bool> IsDoctorAvailable(string doctorId, DateTime appointmentTime)
    {
        var dayOfWeek = appointmentTime.DayOfWeek;
        var timeOfDay = TimeOnly.FromDateTime(appointmentTime);

        // Check if doctor is scheduled to work at this time
        var availability = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId &&
                        a.DayOfWeek == dayOfWeek &&
                        a.StartTime <= timeOfDay &&
                        a.EndTime >= timeOfDay &&
                        a.IsAvailable)
            .AnyAsync();

        if (!availability)
            return false;

        // Check for conflicts with existing appointments
        var existingAppointment = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        (a.RescheduledTime ?? a.ScheduledTime) <= appointmentTime.AddMinutes(30) &&
                        (a.RescheduledTime ?? a.ScheduledTime).AddMinutes(30) >= appointmentTime)
            .AnyAsync();

        return !existingAppointment;
    }

    private async Task<Result> EnforceAppointmentLimitsAsync(string patientId)
    {
        var recentAppointments = await _context.Appointments
            .CountAsync(a => a.PatientId == patientId &&
                             a.ScheduledTime > DateTime.UtcNow &&
                             a.Status == AppointmentStatus.Scheduled);

        if (recentAppointments >= 3)
        {
            _logger.LogWarning("User {PatientId} exceeded max pending appointments.", patientId);

            return Result.Failure(new Error(
                    "Appointment.TooManyPendingAppointments",
                    "You cannot have more than 3 pending appointments",
                    StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }
}
