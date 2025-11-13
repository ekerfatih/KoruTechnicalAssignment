using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestRejectDtoValidator : AbstractValidator<RequestRejectDto>
{
    public RequestRejectDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Red açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Red açıklaması en fazla 500 karakter olabilir.");
    }
}
