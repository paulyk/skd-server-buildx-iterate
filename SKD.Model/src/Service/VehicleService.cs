#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SKD.Model {

    public class VehicleService {

        private readonly SkdContext context;

        public VehicleService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<VehicleLot>> CreateVehicleLot(VehicleLotInput input) {
            var payload = new MutationPayload<VehicleLot>(null);
            payload.Errors = await ValidateCreateVehicleLot(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            // create vehicle records and add to vehicleLot.Vehicles
            var vehicleLot = new VehicleLot { 
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                LotNo = input.LotNo 
            };
            foreach (var vehicleDTO in input.Kits) {
                var vehiclePayload = await CreateVehicleFromKitDTO(vehicleDTO);
                if (vehiclePayload.Errors.Any()) {
                    payload.Errors.AddRange(vehiclePayload.Errors);
                    break;
                }
                vehicleLot.Vehicles.Add(vehiclePayload.Entity);
            }

            // ensure plant code
            context.VehicleLots.Add(vehicleLot);

            // persist
            await context.SaveChangesAsync();
            payload.Entity = vehicleLot;
            return payload;
        }

        public async Task<MutationPayload<VehicleLot>> AssingVehicleKitVin(VehicleKitVinInput dto) {
            var payload = new MutationPayload<VehicleLot>(null);
            payload.Errors = await ValidateAssignVehicleLotVin(dto);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // assign vin
            var vehicleLot = await context.VehicleLots
                .Include(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);
            payload.Entity = vehicleLot;

            vehicleLot.Vehicles.ToList().ForEach(vehicle => {
                var vin = dto.Kits.First(t => t.KitNo == vehicle.KitNo).VIN;
                vehicle.VIN = vin;
            });
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<VehicleTimelineEvent>> CreateVehicleTimelineEvent(VehicleTimelineEventInput dto) {
            var payload = new MutationPayload<VehicleTimelineEvent>(null);
            payload.Errors = await ValidateCreateVehicleTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicle = await context.Vehicles
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == dto.KitNo);

            // mark other timeline events of the same type as removed for this vehicle
            vehicle.TimelineEvents
                .Where(t => t.EventType.Code == dto.EventType.ToString())
                .ToList().ForEach(timelieEvent => {
                    if (timelieEvent.RemovedAt == null) {
                        timelieEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to vehicle
            var newTimelineEvent = new VehicleTimelineEvent {
                EventType = await context.VehicleTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventType.ToString()),
                EventDate = dto.EventDate,
                EventNote = dto.EventNote
            };

            vehicle.TimelineEvents.Add(newTimelineEvent);

            // save
            payload.Entity = newTimelineEvent;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<VehicleLot>> CreateVehicleLotTimelineEvent(VehicleLotTimelineEventInput dto) {
            var payload = new MutationPayload<VehicleLot>(null);
            payload.Errors = await ValidateCreateVehicleLotTimelineEvent(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicleLot = await context.VehicleLots
                .Include(t => t.Vehicles)
                    .ThenInclude(t => t.TimelineEvents)
                    .ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            foreach (var vehicle in vehicleLot.Vehicles) {

                // mark other timeline events of the same type as removed for this vehicle
                vehicle.TimelineEvents
                    .Where(t => t.EventType.Code == dto.EventType.ToString())
                    .ToList().ForEach(timelieEvent => {
                        if (timelieEvent.RemovedAt == null) {
                            timelieEvent.RemovedAt = DateTime.UtcNow;
                        }
                    });

                // create timeline event and add to vehicle
                var newTimelineEvent = new VehicleTimelineEvent {
                    EventType = await context.VehicleTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == dto.EventType.ToString()),
                    EventDate = dto.EventDate,
                    EventNote = dto.EventNote
                };

                vehicle.TimelineEvents.Add(newTimelineEvent);

            }

            // // save
            payload.Entity = vehicleLot;
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateAssignVehicleLotVin(VehicleKitVinInput dto) {
            var errors = new List<Error>();

            var vehicleLot = await context.VehicleLots.AsNoTracking()
                .Include(t => t.Vehicles)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            // lotNO
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
            foreach (var kit in dto.Kits) {
                var existing = await context.Vehicles.AsNoTracking().AnyAsync(t => t.VIN == kit.VIN);
                if (existing) {
                    duplicate_vins.Add(kit.VIN);
                }
            }

            if (duplicate_vins.Any()) {
                errors.Add(new Error("", $"duplicate VIN(s) found {String.Join(", ", duplicate_vins)}"));
                return errors;
            }

            // Wehicles with matching kit numbers not found
            var kit_numbers_not_found = new List<string>();
            foreach (var kit in dto.Kits) {
                var exists = await context.Vehicles.AsNoTracking().AnyAsync(t => t.KitNo == kit.KitNo);
                if (!exists) {
                    kit_numbers_not_found.Add(kit.KitNo);
                }
            }
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
        public async Task<List<Error>> ValidateCreateVehicleLot(VehicleLotInput input) {
            var errors = new List<Error>();

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
            if (plant == null) {
                errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
                return errors;
            }

            var existingLot = await context.VehicleLots.AsNoTracking().FirstOrDefaultAsync(t => t.LotNo == input.LotNo);
            if (existingLot != null) {
                errors.Add(new Error("lotNo", "duplicate vehicle lot"));
                return errors;
            }

            // valid lotNo legnth
            if (input.LotNo.Length != EntityFieldLen.Vehicle_LotNo) {
                errors.Add(new Error("lotNo", $"lotNo must be ${EntityFieldLen.Vehicle_LotNo} characters"));
                return errors;
            }

            // plant code
            if (input.PlantCode.Length != EntityFieldLen.Plant_Code) {
                errors.Add(new Error("plantCode", $"plantCode must be ${EntityFieldLen.Plant_Code} characters"));
                return errors;
            }

            if (!input.Kits.Any()) {
                errors.Add(new Error("", "no vehicles found in lot"));
                return errors;
            }

            if (await context.VehicleLots.AnyAsync(t => t.LotNo == input.LotNo)) {
                errors.Add(new Error("LotNo", "duplicate vehicle lot"));
                return errors;
            }

            // duplicate kitNo
            var duplicateKits = input.Kits.GroupBy(t => t.KitNo).Where(g => g.Count() > 1);
            if (duplicateKits.Any()) {
                var kitNos = String.Join(", ", duplicateKits.Select(g => g.Key));
                errors.Add(new Error("", $"duplicate kitNo in vehicle lot {kitNos}"));
                return errors;
            }

            var modelCodeCount = input.Kits.GroupBy(t => t.ModelCode).Count();
            if (modelCodeCount > 1) {
                errors.Add(new Error("", "Vehicle lot vehicles must have the same model code"));
                return errors;
            }

            // model code exits
            var modelCode = input.Kits.Select(t => t.ModelCode).FirstOrDefault();
            var existingModelCode = await context.VehicleModels.AsNoTracking().FirstOrDefaultAsync(t => t.Code == modelCode);
            if (existingModelCode == null) {
                errors.Add(new Error("", $"vehicle model not found: {modelCode}"));
                return errors;
            }

            return errors;
        }

        public async Task<MutationPayload<Vehicle>> CreateVehicleFromKitDTO(VehicleLotInput.Kit dto) {
            var modelId = await context.VehicleModels
                .Where(t => t.Code == dto.ModelCode)
                .Select(t => t.Id).FirstOrDefaultAsync();

            var vehicle = new Vehicle {
                ModelId = modelId,
                KitNo = dto.KitNo
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

        public async Task<List<Error>> ValidateCreateVehicleTimelineEvent(VehicleTimelineEventInput dto) {
            var errors = new List<Error>();

            var vehicle = await context.Vehicles.AsNoTracking()
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.KitNo == dto.KitNo);
            if (vehicle == null) {
                errors.Add(new Error("KitNo", $"vehicle not found for kitNo: {dto.KitNo}"));
                return errors;
            }

            // duplicate 
            var duplicate = vehicle.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == dto.EventType.ToString())
                .Where(t => t.EventDate == dto.EventDate)
                .Where(t => t.EventNote == dto.EventNote)
                .FirstOrDefault();

            if (duplicate != null) {
                var dateStr = dto.EventDate.ToShortDateString();
                errors.Add(new Error("VIN", $"duplicate vehicle timeline event: {dto.EventType.ToString()} {dateStr} "));
                return errors;
            }

            return errors;
        }

        public async Task<List<Error>> ValidateCreateVehicleLotTimelineEvent(VehicleLotTimelineEventInput dto) {
            var errors = new List<Error>();

            var vehicleLot = await context.VehicleLots.AsNoTracking()
                .Include(t => t.Vehicles).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);

            if (vehicleLot == null) {
                errors.Add(new Error("VIN", $"vehicle lot not found for lotNo: {dto.LotNo}"));
                return errors;
            }

            // duplicate 
            var duplicateTimelineEventsFound = vehicleLot.Vehicles.SelectMany(t => t.TimelineEvents)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == dto.EventType.ToString())
                .Where(t => t.EventDate == dto.EventDate)
                .ToList();

            if (duplicateTimelineEventsFound.Count > 0) {
                var dateStr = dto.EventDate.ToShortDateString();
                errors.Add(new Error("VIN", $"duplicate vehicle timeline event: {dto.LotNo}, Type: {dto.EventType.ToString()} Date: {dateStr} "));
                return errors;
            }

            return errors;
        }
    }
}