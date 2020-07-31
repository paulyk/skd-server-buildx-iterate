using System;

namespace SKD.VCS.Model {

  public class ComponentScan : EntityBase {

    public Guid VehicleComponentId { get; set; }
    public virtual VehicleComponent VehicleComponent { get; set; }
    public string Scan1 { get; set; }
    public string Scan2 { get; set; }      
  }
}