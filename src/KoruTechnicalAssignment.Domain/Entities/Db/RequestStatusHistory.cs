using KoruTechnicalAssignment.Domain.Entities.Enums;
using KoruTechnicalAssignment.Domain.Entities.Identity;

namespace KoruTechnicalAssignment.Domain.Entities.Db {
    public class RequestStatusHistory : Entity {
        public Guid RequestId { get; set; }
        public Request Request { get; set; } = null!;
        public RequestStatus Status { get; set; }
        public string? Reason { get; set; }
        public string ChangedById { get; set; } = null!; 
        public ApplicationUser ChangedBy { get; set; } = null!;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
