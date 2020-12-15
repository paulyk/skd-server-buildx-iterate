
using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Model;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SKD.Server {
    public class Mutation {

        /// <summary>
        /// Create a vehicle lot
        /// </summary>
        public async Task<MutationPayload<VehicleLot>> CreateVehicleLot(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleLotInput input
        ) {
            return await service.CreateVehicleLot(input);
        }

        public async Task<MutationPayload<VehicleModel>> CreateVehicleModel(
            [Service] VehicleModelService service,
            [Service] SkdContext ctx,
            VehicleModelInput input
        ) {
            return await service.CreateVehicleModel(input);
        }

        public async Task<MutationPayload<VehicleLot>> AssignVehicleKitVin(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleKitVinInput input
        ) {
            return await service.AssingVehicleKitVin(input);
        }

        public async Task<MutationPayload<VehicleTimelineEvent>> CreateVehicleTimelineEvent(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleTimelineEventInput input
        ) {
            return await service.CreateVehicleTimelineEvent(input);
        }

        public async Task<MutationPayload<VehicleLot>> CreateVehicleLotTimelineEvent(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleLotTimelineEventInput input
        ) {
            return await service.CreateVehicleLotTimelineEvent(input);
        }

        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationPayload<Component>> SaveComponent(
            [Service] ComponentService service1,
            [Service] SkdContext ctx,
            ComponentInput input
        ) {
            var dto = new Model.ComponentInput {
                Id = ToGuid(input.Id != null ? input.Id : ""),
                Code = input.Code,
                Name = input.Name
            };
            return await service1.SaveComponent(dto);
        }

        /// <summary>
        /// Create or update a production station
        /// </summary>
        public async Task<MutationPayload<ProductionStation>> SaveProductionStation(
            [Service] ProductionStationService service,
            [Service] SkdContext ctx,
            ProductionStationInput input
        ) {
            var dto = new Model.ProductionStationInput {
                Id = ToGuid(input.Id != null ? input.Id : ""),
                Code = input.Code,
                Name = input.Name
            };
            return await service.SaveProductionStation(dto);
        }

        public async Task<MutationPayload<ComponentScan>> CreateComponentScan(
          [Service] ComponentScanService service,
          [Service] SkdContext ctx,
          ComponentScanInput input
        ) {
            var dto = new ComponentScanInput {
                VehicleComponentId = input.VehicleComponentId,
                Scan1 = input.Scan1,
                Scan2 = input.Scan2
            };
            return await service.CreateComponentScan(dto);
        }

        public async Task<MutationPayload<DCWSResponse>> CreateDcwsResponse(
          [Service] DCWSResponseService service,
          [Service] SkdContext ctx,
          DCWWResponseInput input
        ) {
            var dto = new DCWWResponseInput {
                ComponentScanId = input.ComponentScanId,
                ResponseCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage
            };
            return await service.CreateDCWSResponse(dto);
        }

        public async Task<MutationPayload<ShipmentOverviewDTO>> CreateShipment(
            [Service] ShipmentService service,
            [Service] SkdContext ctx,
            ShipmentInput input
        ) => await service.CreateShipment(input);

         public async Task<MutationPayload<BomSummaryOverviewDTO>> CreateBomSummary(
                  [Service] BomSummaryService service,
                  [Service] SkdContext ctx,
                  BomSummaryInput input
        ) => await service.CreateBomSummary(input);

        public async Task<MutationPayload<SnapshotDTO>> GenerateVehicleSnapshotRun(
                  [Service] VehicleSnapshotService service,
                  [Service] SkdContext ctx,
                  VehicleSnapshotInput input
        ) => await service.GenerateSnapshot(input);

       public async Task<MutationPayload<PlantOverviewDTO>> CreatePlant(
                  [Service] PlantService service,
                  [Service] SkdContext ctx,
                  PlantInput input
        ) => await service.CreatePlant(input);

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }
    }
}
