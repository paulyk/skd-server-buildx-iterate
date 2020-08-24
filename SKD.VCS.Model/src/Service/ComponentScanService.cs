using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ComponentScanService {
        private readonly SkdContext context;

        public ComponentScanService(SkdContext ctx) => this.context = ctx;

        public async Task<MutationPayload<ComponentScan>> CreateComponentScan(ComponentScanDTO dto) {
            var componentScan = new ComponentScan {
                Scan1 = dto.Scan1,
                Scan2 = dto.Scan2,
                VehicleComponent = await context.VehicleComponents
                    .Include(t => t.Vehicle).ThenInclude(t => t.VehicleComponents)
                    .FirstOrDefaultAsync(t => t.Id == dto.VehicleComponentId)
            };

            var payload = new MutationPayload<ComponentScan>(componentScan);

            payload.Errors = await ValidateCreateComponentScan<ComponentScan>(componentScan);
            if (payload.Errors.Count() > 0) {
                return payload;
            } 

            // save
            context.ComponentScans.Add(componentScan);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateComponentScan<T>(ComponentScan scan) where T : ComponentScan {
            var errors = new List<Error>();

            var vehicleComponent = scan.VehicleComponent;

            var vehicle = vehicleComponent != null 
                ? await context.Vehicles.AsTracking()
                    .Include(t => t.VehicleComponents)
                    .ThenInclude(vc => vc.ComponentScans)
                    .FirstOrDefaultAsync(t => t.Id == vehicleComponent.VehicleId)
                : null;

            // validate
            if (vehicleComponent == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle component not found"));
                return errors;
            } 

            if (vehicle.ScanLockedAt != null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle locked, scans not allowed"));
                return errors;
            }

            if (string.IsNullOrEmpty(scan.Scan1) && string.IsNullOrEmpty(scan.Scan2)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Scan1, "scan1 and or scan2 required"));
                return errors;
            }

            if (scan.Scan1.Length > EntityMaxLen.ComponentScan_ScanEntry || scan.Scan2.Length > EntityMaxLen.ComponentScan_ScanEntry) {
                errors.Add(ErrorHelper.Create<T>(t => t.Scan1, $"scan entry cannot exceed {EntityMaxLen.ComponentScan_ScanEntry} characters"));
                return errors;
            }

          

            return errors;
        }
    }
}
