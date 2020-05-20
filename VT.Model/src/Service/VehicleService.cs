#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VT.Model {

    public class VehicleService {

        private readonly AppDbContext context;

        public VehicleService(AppDbContext ctx) {
            this.context = ctx;
        }
        public async Task<UpdateVehiclePayload> CreateVehicle(Vehicle vehicle) {

            // ensure vehicle model is same as ID
            if (vehicle.ModelId != null && vehicle.ModelId != Guid.Empty) {
                vehicle.Model = await context.VehicleModels.FirstOrDefaultAsync(t => t.Id == vehicle.ModelId);
            }

            var payload = await ValidateCreateVehicle(vehicle);

            if (payload.Errors.Any()) {
                return payload;
            }

            // add components
            vehicle.Model.ActiveComponentMappings.ToList().ForEach(mapping => {
                if (!vehicle.VehicleComponents.Any(t => t.Component.Id == mapping.ComponentId)) {
                    vehicle.VehicleComponents.Add(new VehicleComponent() {
                        Component = mapping.Component
                    });
                }
            });

            // save
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            payload.Vehicle = vehicle;

            return payload;
        }

        public async Task<UpdateVehiclePayload> ValidateCreateVehicle(Vehicle vehicle) {
            var payload = new UpdateVehiclePayload();
            payload.Vehicle = vehicle;

            if (vehicle.VIN.Trim().Length != EntityMaxLen.Vehicle_VIN) {
                payload.AddError("vin", $"VIN must be exactly {EntityMaxLen.Vehicle_VIN} characters");
            }
            if (await context.Vehicles.AnyAsync(t => t.VIN == vehicle.VIN)) {
                payload.AddError("vin", "Duplicate VIN found");
            }

            // vehicle mode ID empty / not found
            if (vehicle.ModelId == Guid.Empty) {
                payload.AddError("modelId", "Vehicle mode not found. Check Model ID");
            } else if (vehicle.Model == null) {
                payload.AddError("modelId", $"Vehicle Model not found for: model ID: {vehicle.ModelId}");
            }

            // vehicle mode deactivated
            if (vehicle.Model != null && vehicle.Model.RemovedAt != null) {
                payload.AddError("modelId", $"Cannot use a deactivated vehicle model, model CODE: {vehicle.Model.Code}");
            }

            // Lot No
            if (vehicle.LotNo.Trim().Length < EntityMaxLen.Vehicle_LotNo) {
                payload.AddError("kitNo", $"LotNo must be {EntityMaxLen.Vehicle_LotNo} characters");
            } else if (!IsNumeric(vehicle.KitNo)) {
                payload.AddError("kitNo", $"KitNo must be numeric");
            }

            // Kit No
            if (vehicle.KitNo.Trim().Length < EntityMaxLen.Vehicle_KitNo) {
                payload.AddError("kitNo", $"KitNo must be {EntityMaxLen.Vehicle_KitNo} characters");
            } else if (!IsNumeric(vehicle.LotNo)) {
                payload.AddError("LotNo", $"KitNo must be numeric");
            }

            return payload;
        }

        public async Task<IReadOnlyList<Vehicle>> SearchVehicles(string query) {
            query = query.Trim();
            if (query.Length == 0) {
                return new List<Vehicle>();
            }

            // try find exact match
            var exactMatch = await context.Vehicles.FirstOrDefaultAsync(t => t.VIN == query);
            if (exactMatch != null) {
                return new List<Vehicle>() { exactMatch };
            }

            // find where query matches part of vin
            var byVIN = await context.Vehicles.AsNoTracking().Where(t => t.VIN.Contains(query)).ToListAsync();

            // find where matches
            var byModel = await context.Vehicles
                .AsNoTracking()
                .Where(t => t.Model.Code.Contains(query) || t.Model.Name.Contains(query))
                .ToListAsync();

            return byVIN.Union(byModel).ToList();
        }

        private bool IsNumeric(string str) {
            Int32 n;
            return Int32.TryParse(str, out n);
        }
    }
}