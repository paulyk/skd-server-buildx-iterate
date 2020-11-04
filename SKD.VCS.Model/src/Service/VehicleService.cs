#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SKD.VCS.Model {

    public class VehicleService {

        private readonly SkdContext context;

        public VehicleService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<VehicleLot>> CreateVhicleLot(VehicleLotDTO dto) {
            var vehicleLot = new VehicleLot { LotNo = dto.LotNo };
            var payload = new MutationPayload<VehicleLot>(vehicleLot);

            payload.Errors = await ValidateCreateVehicleLot(dto);

            if (payload.Errors.Any()) {
                return payload;
            }

            // create vehicle records and add to vehicleLot.Vehicles
            foreach (var vehicleDTO in dto.VehicleDTOs) {
                var vehiclePayload = await CreateVehicle_Common(vehicleDTO, vehicleLot);
                if (vehiclePayload.Errors.Any()) {
                    payload.Errors.AddRange(vehiclePayload.Errors);
                    break;
                }
                vehicleLot.Vehicles.Add(vehiclePayload.Entity);
            }
            if (payload.Errors.Any()) {
                return payload;
            }

            // persist
            await context.SaveChangesAsync();
            return payload;
        }       

        public async Task<List<Error>> ValidateCreateVehicleLot(VehicleLotDTO dto) {
            var errors = new List<Error>();

            if (!dto.VehicleDTOs.Any()) {
                errors.Add(new Error("", "no vehicles found in lot"));
                return errors;
            }

            if (await context.VehicleLots.AnyAsync(t => t.LotNo == dto.LotNo)) {
                errors.Add(new Error("LotNo", "duplicate vehicle lot"));
                return errors;
            }

            // duplicate vin
            var duplicateVINs = dto.VehicleDTOs.GroupBy(t => t.VIN).Where(g => g.Count() > 1);        
            if (duplicateVINs.Any()) {
                var vins = String.Join(", ", duplicateVINs.Select(g => g.Key));
                errors.Add(new Error("", $"duplicate vin in vehicle lot {vins}"));
                return errors;
            }

            var modelCodeCount = dto.VehicleDTOs.GroupBy(t => t.ModelCode).Count();
            if (modelCodeCount > 1) {
                errors.Add(new Error("", "Vehicle lot vehicles must have the same model code"));
                return errors;
            }

            // model code exits
            var modelCode = dto.VehicleDTOs.Select(t => t.ModelCode).FirstOrDefault();
            var existingModelCode = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == modelCode);
            if (existingModelCode == null) {
                errors.Add(new Error("", $"vehicle model not found: {modelCode}"));
                return errors;
            }

            return errors;
        }

        public async Task<MutationPayload<Vehicle>> CreateVehicle_Common(VehicleDTO dto, VehicleLot? vehicleLot = null) {
            var modelId = await context.VehicleModels
                .Where(t => t.Code == dto.ModelCode)
                .Select(t => t.Id).FirstOrDefaultAsync();

            var vehicle = new Vehicle();
            vehicle.VIN = dto.VIN ?? "";
            vehicle.ModelId = modelId;
            vehicle.LotNo = dto.LotNo ?? "";
            vehicle.KitNo = dto.KitNo ?? "";
            vehicle.Lot = vehicleLot;
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
                // add vehicle components
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
            return payload;
        }

        public async Task<MutationPayload<Vehicle>> ScanLockVehicle(string vin) {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(t => t.VIN == vin);
            var payload = new MutationPayload<Vehicle>(vehicle);

            payload.Errors = ValicateScanLockVehicle<Vehicle>(vehicle);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            vehicle.ScanCompleteAt = DateTime.UtcNow;
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
            if (vehicle.LotNo?.Trim().Length < EntityFieldLen.Vehicle_LotNo) {
                errors.Add(ErrorHelper.Create<T>(t => t.LotNo, $"LotNo must be {EntityFieldLen.Vehicle_LotNo} characters"));
            }

            // Kit No
            if (vehicle.KitNo?.Trim().Length < EntityFieldLen.Vehicle_KitNo) {
                errors.Add(ErrorHelper.Create<T>(t => t.KitNo, $"KitNo must be {EntityFieldLen.Vehicle_KitNo} characters"));
            }

            // vehicle lot
            if (vehicle.Lot == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Lot, "must be linked to vehicle lot"));
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

        /// <summary>
        /// Get existing vehice lot or returns a new one
        /// </summary>
        private async Task<VehicleLot> GetCreateVehicleLot(string lotNo) {
            var vehicleLot = await context.VehicleLots
                .Include(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.LotNo == lotNo);
            if (vehicleLot == null) {
                vehicleLot = new VehicleLot { LotNo = lotNo };
                context.VehicleLots.Add(vehicleLot);
            }
            return vehicleLot;
        }
    }
}