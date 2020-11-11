
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
        /// Create a vehicle lot
        /// </summary>
        public async Task<MutationPayload<VehicleLot>> CreateVehicleLot(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleLotDTO input
        ) {
            return await service.CreateVehicleLot(input);
        }

        public async Task<MutationPayload<VehicleModel>> CreateVehicleModel(
            [Service] VehicleModelService service,
            [Service] SkdContext ctx,
            VehicleModelDTO input
        ) {
            return await service.CreateVehicleModel(input);
        }

        public async Task<MutationPayload<VehicleLot>> AssignVehicleKitVin(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleKitVinDTO input
        ) {
            return await service.AssingVehicleKitVin(input);
        }

        public async Task<MutationPayload<VehicleTimeline>> UpdateVehicleTimeline(
            [Service] VehicleService service,
            [Service] SkdContext ctx,
            VehicleTimelineDTO input
        ) {
            return await service.UpdateVehicleTimeline(input);
        }


        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationPayload<Component>> SaveComponent(
            [Service] ComponentService service1,
            [Service] SkdContext ctx,
            ComponentInput input
        ) {
            var dto = new ComponentDTO {
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
            var dto = new ProductionStationDTO {
                Id = ToGuid(input.Id != null ? input.Id : ""),
                Code = input.Code,
                Name = input.Name
            };
            return await service.SaveProductionStation(dto);
        }

        public async Task<MutationPayload<ComponentScan>> CreateComponentScan(
          [Service] ComponentScanService service,
          [Service] SkdContext ctx,
          ComponentScanDTO input
        ) {
            var dto = new ComponentScanDTO {
                VehicleComponentId = input.VehicleComponentId,
                Scan1 = input.Scan1,
                Scan2 = input.Scan2
            };
            return await service.CreateComponentScan(dto);
        }

        public async Task<MutationPayload<DCWSResponse>> CreateDcwsResponse(
          [Service] DCWSResponseService service,
          [Service] SkdContext ctx,
          DCWWResponseDTO input
        ) {
            var dto = new DCWWResponseDTO {
                ComponentScanId = input.ComponentScanId,
                ResponseCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage
            };
            return await service.CreateDCWSResponse(dto);
        }

        public async Task<MutationPayload<Shipment>> CreateShipment(
            [Service] ShipmentService service,
            [Service] SkdContext ctx,
            ShipmentDTO input
        ) => await service.CreateShipment(input);

        public async Task<MutationPayload<BomSummary>> CreateBomSummary(
                  [Service] BomSummaryService service,
                  [Service] SkdContext ctx,
                  BomSummaryDTO input
        ) => await service.CreateBomSummary(input);

        private Guid ToGuid(string str) {
            Guid gOut;
            Guid.TryParse(str, out gOut);
            return gOut;
        }
    }
}
