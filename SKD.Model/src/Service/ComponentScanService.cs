using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class ComponentScanService {
        private readonly SkdContext context;

        public ComponentScanService(SkdContext ctx) => this.context = ctx;

        public async Task<MutationPayload<ComponentScan>> SaveComponentScan(ComponentScan componentScan) {
            componentScan.TrimStringProperties();
            var payload = new MutationPayload<ComponentScan>(componentScan);

            payload.Errors = await ValidateCreateComponentScan<ComponentScan>(componentScan);
            if (payload.Errors.Count() > 0) {
                return payload;
            } 

            context.ComponentScans.Add(componentScan);

            // save
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateComponentScan<T>(ComponentScan scan) where T : ComponentScan {
            var errors = new List<Error>();

            var vehicleComponent = await context.VehicleComponents
                .AsNoTracking()
                .Include(t => t.Vehicle).ThenInclude(vehicle => vehicle.Model)
                .FirstOrDefaultAsync(t => t.Id == scan.VehicleComponentId);

            if (vehicleComponent == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle component not found"));
                return errors;
            } 

            var vehicle = vehicleComponent.Vehicle;
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
