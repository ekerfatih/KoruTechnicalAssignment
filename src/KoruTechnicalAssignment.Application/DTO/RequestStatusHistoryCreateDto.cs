using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.DTO {
    public class RequestStatusHistoryCreateDto {
        public Guid RequestId { get; set; }
        public string ChangedById { get; set; } = null!;
        public RequestStatus Status { get; set; }
        public string? Reason { get; set; }
    }
}
