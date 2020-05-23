#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VT.Model {

    public class VehicleService {

        private readonly AppDbContext context;

        public VehicleService(AppDbContext ctx) {
            this.context = ctx;
        }
        public async Task<UpdateVehiclePayload> CreateVehicle(Vehicle vehicle) {
            context.Vehicles.Add(vehicle);

            // ensure vehicle model is same as ID            
            if (vehicle.ModelId != null && vehicle.ModelId != Guid.Empty) {
                vehicle.Model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Id == vehicle.ModelId);
            }

            if (vehicle.Model != null) {
                // add components
                vehicle.Model.ActiveComponentMappings.ToList().ForEach(mapping => {
                    if (!vehicle.VehicleComponents.Any(t => t.Component.Id == mapping.ComponentId)) {
                        vehicle.VehicleComponents.Add(new VehicleComponent() {
                            Component = mapping.Component,
                            Sequence = mapping.Sequence
                        });
                    }
                });
            }

            // validate
            var payload = await ValidateCreateVehicle(vehicle);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();

            payload.Vehicle = vehicle;
            return payload;
        }

        public async Task<UpdateVehiclePayload> ValidateCreateVehicle(Vehicle vehicle) {
            var payload = new UpdateVehiclePayload();
            payload.Vehicle = vehicle;

            if (vehicle.VIN.Trim().Length != EntityMaxLen.Vehicle_VIN) {
                payload.AddError("vin", $"VIN must be exactly {EntityMaxLen.Vehicle_VIN} characters");
            }
            if (await context.Vehicles.AnyAsync(t => t.Id != vehicle.Id && t.VIN == vehicle.VIN)) {
                payload.AddError("vin", "Duplicate VIN found");
            }

            // vehicle mode ID empty / not found
            if (vehicle.Model == null) {
                payload.AddError("model", $"Vehicle model not specified");
            }

            // vehicle mode deactivated
            if (vehicle.Model != null && vehicle.Model.RemovedAt != null) {
                payload.AddError("model", $"Vehicle model removed / deactivated: {vehicle.Model.Code}");
            }

            // vehicle components
            if (vehicle.Model != null) {

                if (vehicle.VehicleComponents.Count == 0) {
                    payload.AddError("", "Vehicle components required, but none found");
                } else if (vehicle.Model.ComponentMappings.Count != vehicle.VehicleComponents.Count) {
                    payload.AddError("", $"Vehicle components don't match model component count");
                } else {
                    // vehicle components sequence must match model component sequence
                    var vehicleComponents = vehicle.VehicleComponents.OrderBy(t => t.Sequence).ToList();
                    var modelComponents = vehicle.Model.ComponentMappings.OrderBy(t => t.Sequence).ToList();

                    var zipped = vehicleComponents.Zip(modelComponents, (v, m) => new {
                        vehicle_Seq = v.Sequence,
                        model_Seq = m.Sequence,
                        vehicle_ComponentId = v.Component.Id,
                        model_ComponentId = m.Component.Id
                    }).ToList();

                    // any sequence mismatch
                    if (zipped.Any(item => item.vehicle_Seq != item.model_Seq)) {
                        payload.AddError("", "Vehicle compopnent sequence doesn't match model component sequeunce");
                    }
                    // any component mismatch
                    if (zipped.Any(item => item.vehicle_ComponentId != item.model_ComponentId)) {
                        payload.AddError("", "Vehicle component ID doesn't match model component ID");
                    }
                }
            }

            // Lot No
            if (vehicle.LotNo.Trim().Length < EntityMaxLen.Vehicle_LotNo) {
                payload.AddError("kitNo", $"LotNo must be {EntityMaxLen.Vehicle_LotNo} characters");
            } else if (!IsNumeric(vehicle.LotNo)) {
                payload.AddError("kitNo", $"KitNo must be numeric");
            }

            // Kit No
            if (vehicle.KitNo.Trim().Length < EntityMaxLen.Vehicle_KitNo) {
                payload.AddError("kitNo", $"KitNo must be {EntityMaxLen.Vehicle_KitNo} characters");
            } else if (!IsNumeric(vehicle.KitNo)) {
                payload.AddError("LotNo", $"KitNo must be numeric");
            }

            return payload;
        }


        private bool IsNumeric(string str) {
            Int32 n;
            return Int32.TryParse(str, out n);
        }
    }
}