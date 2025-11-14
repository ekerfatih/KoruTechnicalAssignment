using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestApproveDtoValidator : AbstractValidator<RequestApproveDto> {
    public RequestApproveDtoValidator() {
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason can be at most 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
