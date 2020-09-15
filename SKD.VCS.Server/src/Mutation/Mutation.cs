
using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.VCS.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SKD.VCS.Server {
    public class Mutation {

        /// <summary>
        /// Create a vehicle entry
        /// </summary>
        public async Task<MutationPayload<Vehicle>> CreateVehicle(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleInput input
        ) {
            var dto = new VehicleDTO {
                VIN = input.Vin,
                ModelId = ToGuid(input.ModelId),
                KitNo = input.KitNo,
                LotNo = input.LotNo,
                PlannedBuildAt = input.PlannedBuildAt
            };
            return await service.CreateVehicle(dto);
        }

        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationPayload<Component>> SaveComponent(
            [Service] ComponentService service1,
            [Service] SkdContext ctx,
            ComponentInput input
        ) {
            var component = new ComponentDTO {
                Id = ToGuid(input.Id != null ? input.Id : ""),
                Code = input.Code,
                Name = input.Name
            };
            return await service1.SaveComponent(component);
        }

        /// <summary>
        /// Create or update a production station
        /// </summary>
        public async Task<MutationPayload<ProductionStation>> SaveProductionStation(
            [Service] ProductionStationService service,
            [Service] SkdContext ctx,
            ProductionStationInput input
        ) {
            var productionStation = new ProductionStationDTO {
                Id = ToGuid(input.Id != null ? input.Id : ""),
                Code = input.Code,
                Name = input.Name
            };
            return await service.SaveProductionStation(productionStation);
        }

        public async Task<MutationPayload<ComponentScan>> CreateComponentScan(
          [Service] ComponentScanService service,
          [Service] SkdContext ctx,
          ComponentScanInput input
        ) {
            var componentScanDTO = new ComponentScanDTO {
                VehicleComponentId = input.vehicleComponentId,
                Scan1 = input.Scan1,
                Scan2 = input.Scan2
            };
            return await service.CreateComponentScan(componentScanDTO);
        }

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }
    }
}
