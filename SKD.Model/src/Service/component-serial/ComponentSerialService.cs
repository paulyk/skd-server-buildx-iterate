#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class ComponentSerialService {
        private readonly SkdContext context;

        public ComponentSerialService(SkdContext ctx) => this.context = ctx;

        public async Task<MutationPayload<ComponentSerialDTO>> CaptureComponentSerial(ComponentSerialInput input) {
            input = SwapSerial(input);

            var payload = new MutationPayload<ComponentSerialDTO>(null);

            payload.Errors = await ValidateCaptureComponentSerial<ComponentSerialInput>(input);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            var componentSerial = new ComponentSerial {
                Serial1 = input.Serial1,
                Serial2 = input.Serial2,
                KitComponentId = input.VehicleComponentId
            };

            // Deactivate existing scan if Replace == true
            if (input.Replace) {
                var existintScans = await context.ComponentSerials
                    .Where(t => t.KitComponentId == input.VehicleComponentId && t.RemovedAt == null).ToListAsync();
                existintScans.ForEach(t => t.RemovedAt = DateTime.UtcNow);
            }

            // add             
            context.ComponentSerials.Add(componentSerial);

            // save
            await context.SaveChangesAsync();

            payload.Entity = await context.ComponentSerials
                .Where(t => t.Id == componentSerial.Id)
                .Select(t => new ComponentSerialDTO {
                    ComponentSerialId = t.Id,
                    VIN = t.KitComponent.Kit.VIN,
                    LotNo = t.KitComponent.Kit.Lot.LotNo,
                    ComponentCode = t.KitComponent.Component.Code,
                    ComponentName = t.KitComponent.Component.Name,
                    ProductionStationCode = t.KitComponent.ProductionStation.Code,
                    ProductionStationName = t.KitComponent.ProductionStation.Name,
                    Serial1 = t.Serial1,
                    Serial2 = t.Serial2,
                    VerifiedAt = t.VerifiedAt,
                    CreatedAt = t.CreatedAt
                }).FirstOrDefaultAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCaptureComponentSerial<T>(ComponentSerialInput input) where T : ComponentSerialInput {
            var errors = new List<Error>();

            var targetVehicleCmponent = await context.VehicleComponents
                .Include(t => t.ProductionStation)
                .FirstOrDefaultAsync(t => t.Id == input.VehicleComponentId);

            if (targetVehicleCmponent == null) {
                errors.Add(new Error("VehicleComponentId", $"vehicle component not found: {input.VehicleComponentId}"));
                return errors;
            }

            if (targetVehicleCmponent.RemovedAt != null) {
                errors.Add(new Error("VehicleComponentId", $"vehicle component removed: {input.VehicleComponentId}"));
                return errors;
            }

            // serial numbers blank
            if (input.Serial1 is null or "" && input.Serial2 is null or "") {
                errors.Add(new Error("", "no serial numbers provided"));
                return errors;
            }

            // serial not null and numbers identical 
            if (input.Serial1 is not null or "" && input.Serial1 == input.Serial2) {
                errors.Add(new Error("", "serial 1 and 2 are the same"));
                return errors;
            }

            // component serial entry for this vehicle component 
            var componentSerialForVehicleComponent = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .Include(t => t.KitComponent).ThenInclude(t => t.ProductionStation)
                .Where(t => t.KitComponent.Id == input.VehicleComponentId)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync();

            if (componentSerialForVehicleComponent != null && !input.Replace) {
                var vin = componentSerialForVehicleComponent.KitComponent.Kit.VIN;
                var stationCode = componentSerialForVehicleComponent.KitComponent.ProductionStation.Code;
                var componentCode = componentSerialForVehicleComponent.KitComponent.Component.Code;
                errors.Add(new Error("", $"component serial already captured for this component: {vin}-{stationCode}-{componentCode}"));
                return errors;
            }

            // serial no already in use by different vehicle component
            var componentSerials_with_same_serialNo = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .Where(t => t.RemovedAt == null)
                // different component
                .Where(t => t.KitComponent.Id != targetVehicleCmponent.Id)
                // exclude if same vehicle and same component code   
                // ....... Engine component code will be scanned muliple times)
                .Where(t => !(
                    // same vehicle
                    t.KitComponent.KitId == targetVehicleCmponent.KitId
                    &&
                    // same component
                    t.KitComponent.ComponentId == targetVehicleCmponent.ComponentId
                    )
                )
                // user could point scan serial 2 before serial 1, so we check for both
                .Where(t =>
                    t.Serial1 == input.Serial1 && t.Serial2 == input.Serial2
                    ||
                    t.Serial1 == input.Serial2 && t.Serial2 == input.Serial1
                )
                .ToListAsync();


            if (componentSerials_with_same_serialNo.Count() > 0) {
                errors.Add(new Error("", $"serial number already in use by aonther entry"));
                return errors;
            }

            /* MULTI STATION COMPONENT
            *  Some components serials should be captured repeatedly in consequitive production sations.
            *  They should not be captured out of order
            */

            var preeceedingRequiredComponentEntriesNotCaptured = await context.VehicleComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                // save vehicle         
                .Where(t => t.Kit.Id == targetVehicleCmponent.KitId)
                // same component id
                .Where(t => t.ComponentId == targetVehicleCmponent.ComponentId)
                // preceeding target vehicle component
                .Where(t => t.ProductionStation.Sequence < targetVehicleCmponent.ProductionStation.Sequence)
                // no captured serial entries
                .Where(t => !t.ComponentSerials.Any(u => u.RemovedAt == null))
                .Select(t => new {
                    ProductionStationCode = t.ProductionStation.Code,
                })
                .ToListAsync();

            if (preeceedingRequiredComponentEntriesNotCaptured.Any()) {
                var statonCodes = preeceedingRequiredComponentEntriesNotCaptured
                    .Select(t => t.ProductionStationCode)
                    .Aggregate((a, b) => a + ", " + b);

                errors.Add(new Error("", $"serial numbers for prior stations not captured: {statonCodes}"));
                return errors;
            }

            return errors;
        }

        private ComponentSerialInput SwapSerial(ComponentSerialInput input) {
            input.Serial1 = input.Serial1 is null or "" ? "" : input.Serial1;
            input.Serial2 = input.Serial2 is null or "" ? "" : input.Serial2;

            if (input.Serial1.Trim().Length == 0) {
                return new ComponentSerialInput {
                    VehicleComponentId = input.VehicleComponentId,
                    Serial1 = input.Serial2,
                    Serial2 = ""
                };
            }
            return input;
        }

        public async Task<SerialCaptureVehicleDTO?> GetVehicleInfo_ForSerialCapture(string vin) {
            return await context.Kits
                .Where(t => t.VIN == vin)
                .Select(t => new SerialCaptureVehicleDTO {
                    VIN = t.VIN,
                    LotNo = t.Lot.LotNo,
                    ModelCode = t.Lot.Model.Code,
                    ModelName = t.Lot.Model.Name,
                    Components = t.KitComponents
                        .OrderBy(t => t.ProductionStation.Sequence)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => new SerialCaptureComponentDTO {
                            VehicleComponentId = t.Id,
                            ProductionStationSequence = t.ProductionStation.Sequence,
                            ProductionStationCode = t.ProductionStation.Code,
                            ProductionStationName = t.ProductionStation.Name,
                            ComponentCode = t.Component.Code,
                            ComponentName = t.Component.Name,
                            Serial1 = t.ComponentSerials.Where(u => u.RemovedAt == null).Select(u => u.Serial1).FirstOrDefault(),
                            Serial2 = t.ComponentSerials.Where(u => u.RemovedAt == null).Select(u => u.Serial2).FirstOrDefault(),
                            SerialCapturedAt = t.ComponentSerials.Where(u => u.RemovedAt == null).Select(u => u.CreatedAt).FirstOrDefault(),
                        }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    }
}
