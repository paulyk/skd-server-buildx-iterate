using System;
using System.Linq;

namespace SKD.Model {
    public class EntityBase {
        public Guid Id { get; set; }
        public  DateTime CreatedAt { get; set; }
        public  DateTime? RemovedAt { get; set; }
    
        public EntityBase() {
            CreatedAt = DateTime.UtcNow;
        }

        public void TrimStringProperties() {
            var properties = this.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            foreach(var prop in properties) {
                var value = (string)prop.GetValue(this, null);
                prop.SetValue(this, value.Trim());
            }                
        }
    }
}