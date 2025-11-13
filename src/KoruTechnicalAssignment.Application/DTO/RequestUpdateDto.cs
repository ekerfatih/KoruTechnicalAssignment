namespace KoruTechnicalAssignment.Application.DTO {
    public class RequestUpdateDto {
        public Guid BranchId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly RequestDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
