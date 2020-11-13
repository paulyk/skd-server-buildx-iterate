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

        public async Task<MutationPayload<VehicleLot>> CreateVehicleLot(VehicleLotDTO dto) {
            var vehicleLot = new VehicleLot { LotNo = dto.LotNo };
            var payload = new MutationPayload<VehicleLot>(vehicleLot);
            payload.Errors = await ValidateCreateVehicleLot(dto);
            if (payload.Errors.Any()) {
                return payload;
            }

            // create vehicle records and add to vehicleLot.Vehicles
            foreach (var vehicleDTO in dto.Kits) {
                var vehiclePayload = await CreateVehicleKit(vehicleDTO);
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
            context.VehicleLots.Add(vehicleLot);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<VehicleLot>> AssingVehicleKitVin(VehicleKitVinDTO dto) {
            var payload = new MutationPayload<VehicleLot>(null);
            payload.Errors = await ValidateAssignVehicleLotVin(dto);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // assign vin
            var vehicleLot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);
            vehicleLot.Vehicles.ToList().ForEach(vehicle => {
                var vin = dto.Kits.First(t => t.KitNo == vehicle.KitNo).VIN;
                vehicle.VIN = vin;
            });
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<VehicleTimelineEvent>> CreateVehicleTimelineEvent(VehicleTimelineEventDTO dto) {
            var payload = new MutationPayload<VehicleTimelineEvent>(null);
            payload.Errors = await ValidateCreateVehicleTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicle = await context.Vehicles
                .Include(t => t.TimelineEvents)
                .FirstOrDefaultAsync(t => t.VIN == dto.VIN);

            // mark other timeline events of the same type as removed for this vehicle
            vehicle.TimelineEvents
                .Where(t => t.EventType.Code == dto.EventTypeCode)
                .ToList().ForEach(timelieEvent => {
                    if (timelieEvent.RemovedAt == null) {
                        timelieEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to vehicle
            var newTimelineEvent = new VehicleTimelineEvent {
                EventType = await context.VehicleTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventTypeCode),
                EventDate = dto.EventDate,
            };

            vehicle.TimelineEvents.Add(newTimelineEvent);

            // save
            payload.Entity = newTimelineEvent;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateAssignVehicleLotVin(VehicleKitVinDTO dto) {
            var errors = new List<Error>();

            var vehicleLot = await context.VehicleLots
                .Include(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            if (vehicleLot == null) {
                errors.Add(new Error("lotNo", $"vehicle lot not found: {dto.LotNo}"));
                return errors;
            }

            if (vehicleLot.RemovedAt != null) {
                errors.Add(new Error("lotNo", "vehicle lot marked removed"));
                return errors;
            }

            // invalid VIN(s)
            var validator = new Validator();
            var invalidVins = dto.Kits
                .Select(t => t.VIN)
                .Where(vin => !validator.ValidVIN(vin))
                .ToList();

            if (invalidVins.Any()) {
                errors.Add(new Error("", $"invalid VINs found in lot {String.Join(", ", invalidVins)}"));
                return errors;
            }

            // duplicatev vins
            var duplicate_vins = new List<string>();
            dto.Kits.ToList().ForEach(async kit => {
                var existing = await context.Vehicles.AnyAsync(t => t.VIN == kit.VIN);
                if (existing) {
                    duplicate_vins.Add(kit.VIN);
                }
            });

            if (duplicate_vins.Any()) {
                errors.Add(new Error("", $"duplicate VIN(s) found {String.Join(", ", duplicate_vins)}"));
                return errors;
            }

            // Wehicles with matching kit numbers not found
            var kit_numbers_not_found = new List<string>();
            dto.Kits.ToList().ForEach(async kit => {
                var exists = await context.Vehicles.AnyAsync(t => t.KitNo == kit.KitNo);
                if (!exists) {
                    kit_numbers_not_found.Add(kit.KitNo);
                }
            });

            if (kit_numbers_not_found.Any()) {
                errors.Add(new Error("", $"kit numbers not found {String.Join(", ", kit_numbers_not_found)}"));
                return errors;
            }


            // kit count
            if (vehicleLot.Vehicles.Count() != dto.Kits.Count) {
                errors.Add(new Error("lotNo", $"number of kits {dto.Kits.Count} doesn't match number of kits in lot {vehicleLot.Vehicles.Count}"));
                return errors;
            }

            // duplicate kitNos in payload
            var duplicateKitNos = dto.Kits
                .GroupBy(t => t.KitNo)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.ToList())
                .Select(t => t.KitNo)
                .Distinct();

            if (duplicateKitNos.Count() > 0) {
                errors.Add(new Error("lotNo", $"duplicate kitNo(s) in payload: {String.Join(", ", duplicateKitNos)}"));
                return errors;
            }

            return errors;
        }
        public async Task<List<Error>> ValidateCreateVehicleLot(VehicleLotDTO dto) {
            var errors = new List<Error>();

            var existingLot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);
            if (existingLot != null) {
                errors.Add(new Error("lotNo", "duplicate vehicle lot"));
                return errors;
            }

            if (!dto.Kits.Any()) {
                errors.Add(new Error("", "no vehicles found in lot"));
                return errors;
            }

            if (await context.VehicleLots.AnyAsync(t => t.LotNo == dto.LotNo)) {
                errors.Add(new Error("LotNo", "duplicate vehicle lot"));
                return errors;
            }

            // duplicate kitNo
            var duplicateKits = dto.Kits.GroupBy(t => t.KitNo).Where(g => g.Count() > 1);
            if (duplicateKits.Any()) {
                var kitNos = String.Join(", ", duplicateKits.Select(g => g.Key));
                errors.Add(new Error("", $"duplicate kitNo in vehicle lot {kitNos}"));
                return errors;
            }

            var modelCodeCount = dto.Kits.GroupBy(t => t.ModelCode).Count();
            if (modelCodeCount > 1) {
                errors.Add(new Error("", "Vehicle lot vehicles must have the same model code"));
                return errors;
            }

            // model code exits
            var modelCode = dto.Kits.Select(t => t.ModelCode).FirstOrDefault();
            var existingModelCode = await context.VehicleModels.FirstOrDefaultAsync(t => t.Code == modelCode);
            if (existingModelCode == null) {
                errors.Add(new Error("", $"vehicle model not found: {modelCode}"));
                return errors;
            }

            return errors;
        }

        public async Task<MutationPayload<Vehicle>> CreateVehicleKit(VehicleLotDTO.Kit dto) {
            var modelId = await context.VehicleModels
                .Where(t => t.Code == dto.ModelCode)
                .Select(t => t.Id).FirstOrDefaultAsync();

            var vehicle = new Vehicle {
                ModelId = modelId,
                KitNo = dto.KitNo,
            };

            var payload = new MutationPayload<Vehicle>(vehicle);

            // ensure vehicle.Model set
            if (vehicle.ModelId != Guid.Empty) {
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
            payload.Errors = await ValidateCreateVehicleKit<Vehicle>(vehicle);
            return payload;
        }

        public async Task<List<Error>> ValidateCreateVehicleKit<T>(T vehicle) where T : Vehicle {
            var errors = new List<Error>();
            var validator = new Validator();

            // check duplicate kit no
            if (await context.Vehicles.AnyAsync(t => t.KitNo == vehicle.KitNo && t.Id != vehicle.Id)) {
                errors.Add(ErrorHelper.Create<T>(t => t.VIN, "duplicate KitNo"));
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

            // Kit No
            if (vehicle.KitNo?.Trim().Length < EntityFieldLen.Vehicle_KitNo) {
                errors.Add(ErrorHelper.Create<T>(t => t.KitNo, $"KitNo must be {EntityFieldLen.Vehicle_KitNo} characters"));
            }

            return errors;
        }

        public async Task<List<Error>> ValidateCreateVehicleTimelineEvent(VehicleTimelineEventDTO dto) {
            var errors = new List<Error>();

            var vehicle = await context.Vehicles
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.VIN == dto.VIN);
            if (vehicle == null) {
                errors.Add(new Error("VIN", $"vehicle not found for vin: {dto.VIN}"));
                return errors;
            }

            // duplicate 
            var duplicate = vehicle.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == dto.EventTypeCode)
                .Where(t => t.EventDate == dto.EventDate)
                .FirstOrDefault();

            if (duplicate != null) {
                var dateStr = dto.EventDate.ToShortDateString();
                errors.Add(new Error("VIN", $"duplicate vehicle timeline event: {dto.EventTypeCode} {dateStr} "));
                return errors;
            }

            return errors;
        }
    }
}