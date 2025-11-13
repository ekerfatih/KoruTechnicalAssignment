using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Domain.Entities.Db {
    public class RequestStatusHistory : Entity {
        public Guid RequestId { get; set; }
        public Request Request { get; set; } = null!;
        public RequestStatus Status { get; set; }
        public string? Reason { get; set; }
        public string ChangedById { get; set; } = null!;
        public string? ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
