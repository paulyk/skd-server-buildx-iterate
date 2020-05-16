
using System;

namespace VT.Model {
    public class EntityBase {
        public Guid Id { get; set; }
        public  DateTime CreatedAt { get; set; }
        public  DateTime? RemovedAt { get; set; }
    
        public EntityBase() {
            CreatedAt = DateTime.UtcNow;
        }
    }
}