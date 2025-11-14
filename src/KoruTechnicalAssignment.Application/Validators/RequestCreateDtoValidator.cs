using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Validators;

public sealed class RequestCreateDtoValidator : AbstractValidator<RequestCreateDto> {
    public RequestCreateDtoValidator() {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch selection is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.RequestDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Request date cannot be before today.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time.")
            .Must(RequestTimeRules.IsHalfHourIncrement)
            .WithMessage("Start time must be in 30-minute increments.")
            .Must(RequestTimeRules.IsWithinWorkingHours)
            .WithMessage(RequestTimeRules.WorkingHoursMessage);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.")
            .Must(RequestTimeRules.IsHalfHourIncrement)
            .WithMessage("End time must be in 30-minute increments.")
            .Must(RequestTimeRules.IsWithinWorkingHours)
            .WithMessage(RequestTimeRules.WorkingHoursMessage);

        RuleFor(x => x)
            .Must(x => RequestTimeRules.IntervalWithinWorkingHours(x.StartTime, x.EndTime))
            .WithMessage("Selected times must stay within working hours and cannot include the 13:00-14:00 break.");
    }
}
