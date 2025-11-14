using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestRejectDtoValidator : AbstractValidator<RequestRejectDto> {
    public RequestRejectDtoValidator() {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500)
            .WithMessage("Rejection reason can be at most 500 characters.");
    }
}
