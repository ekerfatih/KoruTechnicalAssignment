using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestApproveDtoValidator : AbstractValidator<RequestApproveDto>
{
    public RequestApproveDtoValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
