using System;
using System.Collections.Generic;
using System.Linq;

namespace SKD.Model {
    public partial class VehicleModel : EntityBase {
           public IQueryable<VehicleModelComponent> ActiveComponentMappings { 
               get {
                   return ModelComponents.Where(t => t.RemovedAt == null).AsQueryable();
               }
           }
    }
}