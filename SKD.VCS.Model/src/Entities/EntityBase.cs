using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SKD.VCS.Model {
    public abstract class EntityBase {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RemovedAt { get; set; }

        public EntityBase() {
            CreatedAt = DateTime.UtcNow;
        }
    }
}