using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.Validators;

internal static class RequestTimeRules {
    private static readonly TimeOnly MorningStart = new(9, 0);
    private static readonly TimeOnly MorningEnd = new(13, 0);
    private static readonly TimeOnly AfternoonStart = new(14, 0);
    private static readonly TimeOnly AfternoonEnd = new(18, 0);
    private static readonly TimeOnly BreakStart = new(13, 0);

    public static bool IsHalfHourIncrement(TimeOnly time) => time.Minute % 30 == 0;

    public static bool IsWithinWorkingHours(TimeOnly time) =>
        (time >= MorningStart && time <= MorningEnd) ||
        (time >= AfternoonStart && time <= AfternoonEnd);

    public static bool IntervalWithinWorkingHours(TimeOnly start, TimeOnly end) =>
        IsWithinWorkingHours(start) &&
        IsWithinWorkingHours(end) &&
        !SpansLunchBreak(start, end);

    private static bool SpansLunchBreak(TimeOnly start, TimeOnly end) =>
        start < BreakStart && end > BreakStart && end > start;

    public static string WorkingHoursMessage =>
        "Working hours are 09:00-13:00 and 14:00-18:00 in 30-minute increments.";
}
