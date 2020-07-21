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

        public async Task<MutationPayload<ComponentScan>> SaveComponent(CreateComponentScan_DTO dto) {
            var entity = new ComponentScan();
            var payload = new MutationPayload<ComponentScan>(entity);

            payload.Errors = await ValidateScan<CreateComponentScan_DTO>(dto);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            return payload;
        }

        public async Task<List<Error>> ValidateScan<T>(CreateComponentScan_DTO dto) where T : CreateComponentScan_DTO {
            var errors = new List<Error>();

            var vehicle = await context.Vehicles.Include(t => t.VehicleComponents).FirstOrDefaultAsync(t => t.VIN == dto.VIN);
            if (vehicle == null) {
                errors.Append(ErrorHelper.Create<T>(t => t.VIN, "vin not found"));
                return errors;
            } 

            if (vehicle.ComponentScanLockedAt != null) {
                errors.Append(ErrorHelper.Create<T>(t => t.VIN, "vin component scan locked"));
                return errors;
            }

            var component = await context.Components.FirstOrDefaultAsync(t => t.Code == dto.ComponentCode && t.RemovedAt == null);
            if (vehicle == null) {
                errors.Append(ErrorHelper.Create<T>(t => t.ComponentCode, "component code not found"));
                return errors;
            }

            var vehicleComponents = vehicle.VehicleComponents.Where(t => t.ComponentId == component.Id && t.RemovedAt == null);
            if (vehicleComponents.Count() == 0) {
                errors.Append(ErrorHelper.Create<T>(t => t.ComponentCode, "component code not required"));
                return errors;
            }

            if (dto.Scan1 == String.Empty && dto.Scan2 == string.Empty) {
                errors.Append(ErrorHelper.Create<T>(t => t.Scan1, "part / serial number required"));
                return errors;
            }

            return errors;
        }
    }
}
