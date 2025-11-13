using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestCreateDtoValidator : AbstractValidator<RequestCreateDto>
{
    public RequestCreateDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Şube seçimi zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.RequestDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Talep tarihi bugünden önce olamaz.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("Başlangıç saati bitiş saatinden küçük olmalıdır.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Bitiş saati başlangıç saatinden büyük olmalıdır.");
    }
}
