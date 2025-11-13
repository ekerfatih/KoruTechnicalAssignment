using KoruTechnicalAssignment.Domain.Entities.Enums;
using KoruTechnicalAssignment.Domain.Entities.Identity;

namespace KoruTechnicalAssignment.Domain.Entities.Db {
    public class Request : Entity {
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public string RequesterId { get; set; } = null!;
        public ApplicationUser Requester { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly RequestDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Draft;
        public ICollection<RequestStatusHistory> History { get; set; } = new List<RequestStatusHistory>();
    }

}