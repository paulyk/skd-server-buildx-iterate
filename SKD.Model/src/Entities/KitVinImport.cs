using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class KitVinImport : EntityBase {
        public Guid PlantId { get; set; }
        public Plant Plant { get; set; }
        public int Sequence { get; set; }
        public string GSDB_Code { get; set; }

        public ICollection<KitVin> KitVins { get; set; }
    }
}