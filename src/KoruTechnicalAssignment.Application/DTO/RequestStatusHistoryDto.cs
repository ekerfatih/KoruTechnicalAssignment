using KoruTechnicalAssignment.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoruTechnicalAssignment.Application.DTO {
    public class RequestStatusHistoryDto {
        public Guid Id { get; set; }
        public RequestStatus Status { get; set; }
        public string ChangedBy { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
