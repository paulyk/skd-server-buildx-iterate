using System;

namespace SKD.Model {

  public class VehicleComponentScan : EntityBase {

    public Guid VehicleComponentId { get; set; }
    public virtual VehicleComponent VehicleComponent { get; set; }
    public string Scan1 { get; set; }
    public string Scan2 { get; set; }
      
  }
}