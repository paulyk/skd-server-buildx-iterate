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
            var vehicleLot = await context.VehicleLots.FirstOrDefaultAsync(t => t.LotNo == dto.LotNo);
            var payload = new MutationPayload<VehicleLot>(null);
            payload.Errors = await ValidateAssignVehicleLotVin(dto);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // assign vind
            vehicleLot.Vehicles.ToList().ForEach(vehicle => {
                var vin = dto.Kits.First(t => t.KitNo == vehicle.KitNo).VIN;
                vehicle.VIN = vin;
            });
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<MutationPayload<VehicleTimeline>> UpdateVehicleTimeline(VehicleTimelineDTO dto) {
            var payload = new MutationPayload<VehicleTimeline>(null);
            payload.Errors = await ValidateUpdateVehicleTimeline(dto);
            if (payload.Errors.Count > 0) {
                return payload;
            }

            var vehicle = await context.Vehicles
                .Include(t => t.Timeline)
                .FirstOrDefaultAsync(t => t.VIN == dto.VIN);

            if (vehicle.Timeline == null) {
                vehicle.Timeline = new VehicleTimeline();
            }

            // update
            switch (dto.DateType) {
                case TimelineOption.CUSTOM_RECEIVED:
                    vehicle.Timeline.CustomReceivedAt = dto.Date;
                    break;
                case TimelineOption.PLAN_BUILD:
                    vehicle.Timeline.PlanBuildAt = dto.Date;
                    break;
                case TimelineOption.BUILD_COMPLETED:
                    vehicle.Timeline.BuildCompletedAt = dto.Date;
                    break;
                case TimelineOption.GATE_RELEASE:
                    vehicle.Timeline.GateRleaseAt = dto.Date;
                    break;
                case TimelineOption.WHOLESALE:
                    vehicle.Timeline.WholeStateAt = dto.Date;
                    break;
            }
            // importan modified date set
            vehicle.Timeline.ModifiedAt = DateTime.UtcNow;

            payload.Entity = vehicle.Timeline;
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

            // any vins alread assigned
            if (vehicleLot.Vehicles.Any(t => t.VIN?.Length > 0)) {
                errors.Add(new Error("lotNo", "vehicle lot VINs already assigned"));
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

            // kitNos not foound
            var dto_kitNos = dto.Kits.OrderBy(t => t.KitNo).Select(t => t.KitNo).ToList();
            var vehicleLot_Kits = vehicleLot.Vehicles.OrderBy(t => t.KitNo).Select(t => t.KitNo).ToList();
            var kits_not_in_vehicleLot = dto_kitNos.Where(dto_kitNo => !vehicleLot_Kits.Any(vehicleKitNo => vehicleKitNo == dto_kitNo));

            if (kits_not_in_vehicleLot.Any()) {
                errors.Add(new Error("", $"kit numbers not found in lot {String.Join(", ", kits_not_in_vehicleLot)}"));
                return errors;
            }

            // valid vins
            var validator = new Validator();
            var invalidVins = dto.Kits
                .Select(t => t.VIN)
                .Where(vin => !validator.ValidVIN(vin))
                .ToList();

            if (invalidVins.Any()) {
                errors.Add(new Error("", $"invalid VINs found in lot {String.Join(", ", invalidVins)}"));
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

        public async Task<List<Error>> ValidateUpdateVehicleTimeline(VehicleTimelineDTO dto) {
            var errors = new List<Error>();

            var vehicle = await context.Vehicles.Include(t => t.Timeline).FirstOrDefaultAsync(t => t.VIN == dto.VIN);
            if (vehicle == null) {
                errors.Add(new Error("VIN", $"vehicle not found for vin: {dto.VIN}"));
                return errors;
            }

            // short circuit of timeline not yet set
            if (vehicle.Timeline == null) {
                return errors;
            }

            var timeline = vehicle.Timeline;


            if (dto.DateType == TimelineOption.PLAN_BUILD) {
                if (timeline.CustomReceivedAt == (DateTime?)null) {
                    errors.Add(new Error("Date", "custom received required before plan build date"));
                } else if (dto.Date <= timeline.CustomReceivedAt.Value) {
                    errors.Add(new Error("Date", "plan build cannot come before custom received date"));
                }
                return errors;
            }

            if (dto.DateType == TimelineOption.BUILD_COMPLETED) {
                if (timeline.PlanBuildAt == (DateTime?)null) {
                    errors.Add(new Error("Date", "plan build required before build completed"));
                } else if (dto.Date <= timeline.PlanBuildAt.Value) {
                    errors.Add(new Error("Date", "plan build cannot come before build complete"));
                }
                return errors;
            }

            return errors;
        }
    }
}