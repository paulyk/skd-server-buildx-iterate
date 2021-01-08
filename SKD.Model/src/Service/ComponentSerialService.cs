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
            input = SwapAndTrimSerial(input);

            var payload = new MutationPayload<ComponentSerialDTO>(null);

            payload.Errors = await ValidateCaptureComponentSerial<ComponentSerialInput>(input);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            var componentSerial = new ComponentSerial {
                Serial1 = input.Serial1,
                Serial2 = input.Serial2,
                VehicleComponentId = input.VehicleComponentId
            };

            // Deactivate existing scan if Replace == true
            if (input.Replace) {
                var existintScans = await context.ComponentSerials
                    .Where(t => t.VehicleComponentId == input.VehicleComponentId && t.RemovedAt == null).ToListAsync();
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
                    VIN = t.VehicleComponent.Vehicle.VIN,
                    LotNo = t.VehicleComponent.Vehicle.Lot.LotNo,
                    ComponentCode = t.VehicleComponent.Component.Code,
                    ComponentName = t.VehicleComponent.Component.Name,
                    Serial1 = t.Serial1,
                    Serial2 = t.Serial2,
                    CreatedAt = t.CreatedAt
                }).FirstOrDefaultAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCaptureComponentSerial<T>(ComponentSerialInput input) where T : ComponentSerialInput {
            var errors = new List<Error>();

            var vehicleComponent = await context.VehicleComponents.FirstOrDefaultAsync(t => t.Id == input.VehicleComponentId);
            if (vehicleComponent == null) {
                errors.Add(new Error("VehicleComponentId", $"vehicle component not found: {input.VehicleComponentId}"));
                return errors;
            }

            if (vehicleComponent.RemovedAt != null) {
                errors.Add(new Error("VehicleComponentId", $"vehicle component removed: {input.VehicleComponentId}"));
                return errors;
            }

            // serial numbers blank
            if (String.IsNullOrEmpty(input.Serial1) && String.IsNullOrEmpty(input.Serial2)) {
                errors.Add(new Error("", "no serial numbers provided"));
                return errors;
            }

            // component serial entry for this vehicle component 
            var componentSerialForVehicleComponent = await context.ComponentSerials
                .Include(t => t.VehicleComponent).ThenInclude(t => t.Vehicle)
                .Include(t => t.VehicleComponent).ThenInclude(t => t.Component)
                .Include(t => t.VehicleComponent).ThenInclude(t => t.ProductionStation)
                .Where(t => t.VehicleComponent.Id == input.VehicleComponentId)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync();

            if (componentSerialForVehicleComponent != null && !input.Replace) {
                var vin = componentSerialForVehicleComponent.VehicleComponent.Vehicle.VIN;
                var stationCode = componentSerialForVehicleComponent.VehicleComponent.ProductionStation.Code;
                var componentCode = componentSerialForVehicleComponent.VehicleComponent.Component.Code;
                errors.Add(new Error("", $"component serial already captured for this component: {vin}-{stationCode}-{componentCode}"));
                return errors;
            }

            // serial no already in use by different vehicle component
            var componentSerial_for_fifferent_VehicleComponent = await context.ComponentSerials
                .Where(t => t.RemovedAt == null)
                .Where(t => t.VehicleComponent.Id != vehicleComponent.Id)
                .Where(t =>
                    t.Serial1 == input.Serial1 && t.Serial2 == input.Serial2
                    ||
                    t.Serial1 == input.Serial2 && t.Serial2 == input.Serial1
                )
                .FirstOrDefaultAsync();

            if (componentSerial_for_fifferent_VehicleComponent != null) {
                errors.Add(new Error("", $"serial number already in use by aonther entry"));
                return errors;
            }

            return errors;
        }

        private ComponentSerialInput SwapAndTrimSerial(ComponentSerialInput input) {
            input.Serial1 = String.IsNullOrEmpty(input.Serial1) ? "" : input.Serial1.Trim();
            input.Serial2 = String.IsNullOrEmpty(input.Serial2) ? "" : input.Serial2.Trim();

            if (input.Serial1.Trim().Length == 0) {
                return new ComponentSerialInput {
                    VehicleComponentId = input.VehicleComponentId,
                    Serial1 = input.Serial2,
                    Serial2 = ""
                };
            }
            return input;
        }
    }
}
