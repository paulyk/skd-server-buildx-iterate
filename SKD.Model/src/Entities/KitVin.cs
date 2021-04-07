using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class KitVin : EntityBase {
        public Guid KitVinImportId { get; set; }
        public KitVinImport KitVinImport { get; set; }

        public Guid KitId { get; set; }
        public Kit Kit { get; set; }
        
        public string VIN { get; set; }
    }
    
}