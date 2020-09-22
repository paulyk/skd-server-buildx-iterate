#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class VehicleService {

        private readonly SkdContext context;

        public VehicleService(SkdContext ctx) {
            this.context = ctx;
        }
        public async Task<MutationPayload<Vehicle>> CreateVehicle(VehicleDTO dto) {
            var vehicle = new Vehicle();
            vehicle.VIN = dto.VIN;
            vehicle.ModelId = dto.ModelId;
            vehicle.LotNo = dto.LotNo;
            vehicle.KitNo = dto.KitNo;
            vehicle.PlannedBuildAt = dto.PlannedBuildAt;

            var payload = new MutationPayload<Vehicle>(vehicle);
            context.Vehicles.Add(vehicle);

            // ensure vehicle.Model set
            if (vehicle.ModelId != null && vehicle.ModelId != Guid.Empty) {
                vehicle.Model = await context.VehicleModels
                    .Include(t => t.ModelComponents).ThenInclude(t => t.Component)
                    .Include(t => t.ModelComponents).ThenInclude(t => t.ProductionStation)
                    .FirstOrDefaultAsync(t => t.Id == vehicle.ModelId);
            }

            if (vehicle.Model != null) {
                // add components
                var modelCOmponents = vehicle.Model.ModelComponents.Where(t => t.RemovedAt == null).ToList();

                modelCOmponents.ForEach(mapping => {                    
                    vehicle.VehicleComponents.Add(new VehicleComponent() {
                        Component = mapping.Component,
                        ProductionStationId = mapping.ProductionStationId,
                        CreatedAt = vehicle.CreatedAt
                    });
                });
            }

            // validate
            payload.Errors = await ValidateCreateVehicle<Vehicle>(vehicle);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<Vehicle>> ScanLockVehicle(string vin) {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(t => t.VIN == vin);
            var payload = new MutationPayload<Vehicle>(vehicle);

            payload.Errors = ValicateScanLockVehicle<Vehicle>(vehicle);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            vehicle.ScanLockedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return payload;
        }
        public async Task<List<Error>> ValidateCreateVehicle<T>(T vehicle) where T : Vehicle {
            var errors = new List<Error>();
            var validator = new Validator();

            // vin format
            if (!validator.ValidVIN(vehicle.VIN)) {
                errors.Add(ErrorHelper.Create<T>(t => t.VIN, $"Invalid VIN format"));
            }
            // check duplicate vin
            if (await context.Vehicles.AnyAsync(t => t.VIN == vehicle.VIN && t.Id != vehicle.Id)) {
                errors.Add(ErrorHelper.Create<T>(t => t.VIN, "Duplicate VIN"));
            }

            // vehicle mode ID empty / not found
            if (vehicle.Model == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Model, $"Vehicle model not specified"));
            }

            // vehicle mode deactivated
            if (vehicle.Model != null && vehicle.Model.RemovedAt != null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Model, $"Vehicle model removed / deactivated: {vehicle.Model.Code}"));
            }

            // vehicle components
            if (vehicle.Model != null) {
                if (vehicle.VehicleComponents.Count == 0) {
                    errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponents, "Vehicle components required, but none found"));
                } else if (vehicle.Model.ModelComponents.Where(t => t.RemovedAt == null).Count() != vehicle.VehicleComponents.Count) {
                    errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponents, $"Vehicle components don't match model component count"));
                } else {
                    // vehicle components must match model component
                    var vehicleComponents = vehicle.VehicleComponents.OrderBy(t => t.Component.Code).ToList();
                    var modelComponents = vehicle.Model.ModelComponents.OrderBy(t => t.Component.Code).ToList();

                    var zipped = vehicleComponents.Zip(modelComponents, (v, m) => new {
                        vehicle_ComponentId = v.Component.Id,
                        model_ComponentId = m.Component.Id
                    }).ToList();

                    // any component mismatch
                    if (zipped.Any(item => item.vehicle_ComponentId != item.model_ComponentId)) {
                        errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponents, "Vehicle component ID doesn't match model component ID"));
                    }
                }
            }

            // Lot No
            if (vehicle.LotNo.Trim().Length < EntityFieldLen.Vehicle_LotNo) {
                errors.Add(ErrorHelper.Create<T>(t => t.KitNo, $"LotNo must be {EntityFieldLen.Vehicle_LotNo} characters"));
            } else if (!IsNumeric(vehicle.LotNo)) {
                errors.Add(ErrorHelper.Create<T>(t => t.LotNo, $"KitNo must be numeric"));
            }

            // Kit No
            if (vehicle.KitNo.Trim().Length < EntityFieldLen.Vehicle_KitNo) {
                errors.Add(ErrorHelper.Create<T>(t => t.KitNo, $"KitNo must be {EntityFieldLen.Vehicle_KitNo} characters"));
            } else if (!IsNumeric(vehicle.KitNo)) {
                errors.Add(ErrorHelper.Create<T>(t => t.LotNo, $"KitNo must be numeric"));
            }

            return errors;
        }

        public List<Error> ValicateScanLockVehicle<T>(T vehicle) where T : Vehicle {
            var errors = new List<Error>();

            var unscannedComponents = vehicle.VehicleComponents.Any(t => !t.ComponentScans.Any());
            if (unscannedComponents) {
                errors.Add(ErrorHelper.Create<T>(t => t.LotNo, $"found vehicle components with not scans"));
            }
            var unverifiedScans = vehicle.VehicleComponents.Any(t => t.ScanVerifiedAt == null);
            if (unscannedComponents) {
                errors.Add(ErrorHelper.Create<T>(t => t.LotNo, $"not vehicle components have verified scans "));
            }

            return errors;

        }
        private bool IsNumeric(string str) {
            Int32 n;
            return Int32.TryParse(str, out n);
        }
    }
}