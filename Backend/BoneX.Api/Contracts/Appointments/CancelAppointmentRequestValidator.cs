namespace BoneX.Api.Contracts.Appointments;


public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Cancellation reason is required and must not exceed 500 characters.");
    }
}
