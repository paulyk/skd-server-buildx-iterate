
using System;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Model;
using SKD.Dcws;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Data;

namespace SKD.Server {
    public class Mutation {

        public async Task<MutationPayload<VehicleModel>> CreateVehicleModel(
            [Service] VehicleModelService service,
            VehicleModelInput input
        ) => await service.CreateVehicleModel(input);

        public async Task<MutationPayload<Lot>> AssingKitVin(
            [Service] KitService service,
            AssignKitVinInput input
        ) => await service.AssingKitVin(input);

        public async Task<MutationPayload<KitTimelineEvent>> CreateKitTimelineEvent(
            [Service] KitService service,
            KitTimelineEventInput input
        ) => await service.CreateKitTimelineEvent(input);

        public async Task<MutationPayload<Lot>> CreateLotTimelineEvent(
            [Service] KitService service,
            LotTimelineEventInput input
        ) => await service.CreateLotTimelineEvent(input);


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

        public async Task<MutationPayload<ComponentSerialDTO>> CaptureComponentSerial(
              [Service] ComponentSerialService service,
              ComponentSerialInput input
            ) {
            return await service.CaptureComponentSerial(input);
        }

        public async Task<MutationPayload<DcwsResponse>> CreateDcwsResponse(
          [Service] DCWSResponseService service,
          DcwsComponentResponseInput input
        ) {
            var dto = new DcwsComponentResponseInput {
                VehicleComponentId = input.VehicleComponentId,
                ResponseCode = input.ResponseCode,
                ErrorMessage = input.ErrorMessage
            };
            return await service.SaveDcwsComponentResponse(dto);
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

        public async Task<MutationPayload<SnapshotDTO>> GenerateKitSnapshotRun(
            [Service] KitSnapshotService service,
            KitSnapshotInput input
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

        public async Task<MutationPayload<DcwsResponse>> VerifyComponentSerial(
            [Service] DcwsService dcwsService,
            [Service] DCWSResponseService dcwsResponseService,
            [Service] SkdContext context,
            Guid kitComponentId
        ) {
            var componentSerial = await context.ComponentSerials
                .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                .Include(t => t.KitComponent).ThenInclude(t => t.Component)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.KitComponentId == kitComponentId)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync();

            var input = new SubmitDcwsComponentInput {
                VIN = componentSerial.KitComponent.Kit.VIN,
                ComponentTypeCode = componentSerial.KitComponent.Component.Code,
                Serial1 = componentSerial.Serial1,
                Serial2 = componentSerial.Serial2
            };

            var submitDcwsComponentResponse = await dcwsService.SubmitDcwsComponent(input);
            var dcwsResponsePayload = await dcwsResponseService.SaveDcwsComponentResponse(new DcwsComponentResponseInput {
                VehicleComponentId = kitComponentId,
                ResponseCode = submitDcwsComponentResponse.ProcessExceptionCode,
            });

            return dcwsResponsePayload;
        }

        public async Task<MutationPayload<ReceiveHandlingUnitPayload>> SetHandlingUnitReceived(
            [Service] HandlingUnitService service,
            ReceiveHandlingUnitInput input
        ) => await service.SetHandlingUnitReceived(input);
    }
}
