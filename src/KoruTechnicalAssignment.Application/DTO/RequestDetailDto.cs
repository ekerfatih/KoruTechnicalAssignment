using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.DTO {
    public class RequestDetailDto {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string RequesterName { get; set; } = null!;
        public DateOnly RequestDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public RequestStatus Status { get; set; }
        public List<RequestStatusHistoryDto> History { get; set; } = new();
    }
}
