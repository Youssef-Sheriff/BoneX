namespace BoneX.Api.Contracts.Appointments;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{

    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();

        RuleFor(x => x.ScheduledTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Scheduled Time must be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
