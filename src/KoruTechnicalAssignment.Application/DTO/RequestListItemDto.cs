using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.DTO {
    public class RequestListItemDto {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string RequesterName { get; set; } = null!;
        public DateOnly RequestDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public RequestStatus Status { get; set; }
    }
}
