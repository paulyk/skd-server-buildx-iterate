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

        public async Task<MutationPayload<ComponentSerialDTO>> CaptureComponentSerial(ComponentSerialInput dto) {
            var payload = new MutationPayload<ComponentSerialDTO>(null);

            payload.Errors = await ValidateCaptureComponentSerial<ComponentSerialInput>(dto);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // swap if scan1 empty
            if (dto.Serial1.Trim().Length == 0) {
                dto.Serial1 = dto.Serial2;
                dto.Serial2 = null;
            }

            var componentSerial = new ComponentSerial {
                Serial1 = dto.Serial1,
                Serial2 = dto.Serial2,
                VehicleComponentId = dto.VehicleComponentId
            };


            // Deactivate existing scan if Replace == true
            if (dto.Replace) {
                var existintScans = await context.ComponentSerials
                    .Where(t => t.VehicleComponentId == dto.VehicleComponentId && t.RemovedAt == null).ToListAsync();                
                existintScans.ForEach(t => t.RemovedAt = DateTime.UtcNow);
            }

            // add             
            context.ComponentSerials.Add(componentSerial);

            // save
            await context.SaveChangesAsync();

            payload.Entity = await context.ComponentSerials
                .Where(t => t.Id == componentSerial.Id)
                .Select(t => new ComponentSerialDTO {
                    Id = t.Id,
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

        public async Task<List<Error>> ValidateCaptureComponentSerial<T>(ComponentSerialInput scan) where T : ComponentSerialInput {
            var errors = new List<Error>();

            var vehicleComponent = await context.VehicleComponents.AsNoTracking()
                    .Include(t => t.Component)
                    .Include(t => t.ProductionStation)
                    .Include(t => t.ComponentSerials)
                .FirstOrDefaultAsync(t => t.Id == scan.VehicleComponentId);

            // vehicle component id
            if (vehicleComponent == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle component not found"));
                return errors;
            }

            var vehicle = vehicleComponent != null
                ? await context.Vehicles.AsNoTracking()
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.Component)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ProductionStation)
                    .Include(t => t.VehicleComponents).ThenInclude(t => t.ComponentSerials)
                    .FirstOrDefaultAsync(t => t.Id == vehicleComponent.VehicleId)
                : null;

            // scan 1 || scan 2 set
            if (string.IsNullOrEmpty(scan.Serial1) && string.IsNullOrEmpty(scan.Serial2)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Serial1, "serial1 and or serial2 required"));
                return errors;
            }

            if (scan.Serial1?.Length > 0 && scan.Serial1?.Length < EntityFieldLen.ComponentScan_ScanEntry_Min
                ||
                scan.Serial2?.Length > 0 && scan.Serial2?.Length < EntityFieldLen.ComponentScan_ScanEntry_Min) {

                errors.Add(ErrorHelper.Create<T>(t => t.Serial1, $"scan entry length min {EntityFieldLen.ComponentScan_ScanEntry_Min} characters"));
                return errors;
            }

            // scan length
            if (scan.Serial1?.Length > EntityFieldLen.ComponentScan_ScanEntry || scan.Serial2?.Length > EntityFieldLen.ComponentScan_ScanEntry) {
                errors.Add(ErrorHelper.Create<T>(t => t.Serial1, $"scan entry length max {EntityFieldLen.ComponentScan_ScanEntry} characters"));
                return errors;
            }

            // cannot add component to vehicle component / unless "replace" mode
            if (vehicleComponent.ComponentSerials.Any(t => t.RemovedAt == null)) {
                if (!scan.Replace) {
                    errors.Add(new Error("", "Existing scan found"));
                    return errors;
                }
            }

            // Get any vehicle component with same code in preceeding production stationss
            var preceedingVehicleComponents = vehicle.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .Where(t => t.Component.Code == vehicleComponent.Component.Code)
                .Where(t => t.ProductionStation.SortOrder < vehicleComponent.ProductionStation.SortOrder)
                .ToList();

            var preceeding_Unscanned_Stations = preceedingVehicleComponents
                .Where(t => !t.ComponentSerials.Any(t => t.RemovedAt == null))
                .Select(t => t.ProductionStation.Code).ToList();

            if (preceeding_Unscanned_Stations.Any()) {
                var station_codes = String.Join(", ", preceeding_Unscanned_Stations);
                errors.Add(new Error("", $"Missing scan for {station_codes}"));
                return errors;
            }

            return errors;
        }
    }
}
