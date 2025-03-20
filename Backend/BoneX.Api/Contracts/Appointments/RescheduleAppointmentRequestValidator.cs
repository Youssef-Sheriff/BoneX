namespace BoneX.Api.Contracts.Appointments;

public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.NewTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New appointment time must be in the future.");
    }
}
