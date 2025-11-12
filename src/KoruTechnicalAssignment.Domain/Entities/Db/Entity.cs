using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoruTechnicalAssignment.Domain.Entities.Db {
    public abstract class Entity {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
