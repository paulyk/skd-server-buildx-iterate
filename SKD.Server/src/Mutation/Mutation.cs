
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
            VehicleLotInput input
        ) {
            return await service.CreateVehicleLot(input);
        }

        public async Task<MutationPayload<VehicleModel>> CreateVehicleModel(
            [Service] VehicleModelService service,
            VehicleModelInput input
        ) {
            return await service.CreateVehicleModel(input);
        }

        public async Task<MutationPayload<VehicleLot>> AssignVehicleKitVin(
            [Service] VehicleService service,
            AssignKitVinInput input
        ) {
            return await service.AssingVehicleKitVin(input);
        }

        public async Task<MutationPayload<VehicleTimelineEvent>> CreateVehicleTimelineEvent(
            [Service] VehicleService service,
            VehicleTimelineEventInput input
        ) {
            return await service.CreateVehicleTimelineEvent(input);
        }

        public async Task<MutationPayload<VehicleLot>> CreateVehicleLotTimelineEvent(
            [Service] VehicleService service,
            VehicleLotTimelineEventInput input
        ) {
            return await service.CreateVehicleLotTimelineEvent(input);
        }

        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationPayload<Component>> SaveComponent(
            [Service] ComponentService service1,
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
          DCWWResponseInput input
        ) {
            var dto = new DCWWResponseInput {
                ComponentScanId = input.ComponentScanId,
                ResponseCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage
            };
            return await service.CreateDCWSResponse(dto);
        }

        public async Task<MutationPayload<ShipmentOverviewDTO>> ImportShipment(
            [Service] ShipmentService service,
            ShipmentInput input
        ) => await service.ImportShipment(input);

        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotKits(
                 [Service] BomService service,
                 BomLotKitInput input
       ) => await service.ImportBomLotKits(input);

        public async Task<MutationPayload<BomOverviewDTO>> ImportBomLotParts(
                 [Service] BomService service,
                 BomLotPartInput input
       ) => await service.ImportBomLotParts(input);

        public async Task<MutationPayload<SnapshotDTO>> GenerateVehicleSnapshotRun(
                  [Service] VehicleSnapshotService service,
                  VehicleSnapshotInput input
        ) => await service.GenerateSnapshot(input);

        public async Task<MutationPayload<PlantOverviewDTO>> CreatePlant(
                   [Service] PlantService service,
                   PlantInput input
         ) => await service.CreatePlant(input);

        public async Task<MutationPayload<LotPartDTO>> CreateLotPartQuantityReceived(
            [Service] LotPartService service,
            ReceiveLotPartInput input
        ) => await service.CreateLotPartQuantityReceived(input);

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }
    }
}
