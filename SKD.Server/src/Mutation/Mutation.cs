
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Dcws;
using HotChocolate;
using System.Threading.Tasks;
using SKD.Service;
using SKD.Model;
using HotChocolate.Types;

namespace SKD.Server {


    public class Mutation {

        public async Task<MutationResult<PCV>> SavePcv(
            [Service] PcvService service,
            PcvInput input
        ) => await service.Save(input);

        public async Task<MutationResult<PCV>> CreatePcvFromExisting(
            [Service] PcvService service,
            PcvFromExistingInput input
        ) => await service.CreateFromExisting(input);

        public async Task<MutationResult<KitTimelineEvent>> CreateKitTimelineEvent(
            [Service] KitService service,
            KitTimelineEventInput input
        ) => await service.CreateKitTimelineEvent(input);

        public async Task<MutationResult<Lot>> CreateLotTimelineEvent(
            [Service] KitService service,
            LotTimelineEventInput input
        ) => await service.CreateLotTimelineEvent(input);


        /// <summary>
        /// Create or update a component
        /// </summary>
        public async Task<MutationResult<Component>> SaveComponent(
                [Service] ComponentService service1,
                ComponentInput input
            ) {
            return await service1.SaveComponent(input);
        }

        /// <summary>
        /// Create or update a production station
        /// </summary>
        public async Task<MutationResult<ProductionStation>> SaveProductionStation(
                [Service] ProductionStationService service,
                ProductionStationInput input
            ) {
            return await service.SaveProductionStation(input);
        }

        public async Task<MutationResult<ComponentSerialDTO>> CaptureComponentSerial(
              [Service] ComponentSerialService service,
              ComponentSerialInput input
            ) {
            return await service.SaveComponentSerial(input);
        }

        public async Task<MutationResult<DcwsResponse>> CreateDcwsResponse(
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

        public async Task<MutationResult<ShipmentOverviewDTO>> ImportShipment(
            [Service] ShipmentService service,
            ShipFile input
        ) => await service.ImportShipment(input);

        public async Task<MutationResult<KitVinImport>> ImportVIN(
            [Service] KitService service,
            VinFile input
        ) => await service.ImportVIN(input);

        public async Task<MutationResult<BomOverviewDTO>> ImportBom(
            [Service] BomService service,
            BomFile input
       ) => await service.ImportBom(input);

        public async Task<MutationResult<SnapshotDTO>> GenerateKitSnapshotRun(
            [Service] KitSnapshotService service,
            KitSnapshotInput input
        ) => await service.GenerateSnapshot(input);

        public async Task<MutationResult<PlantOverviewDTO>> CreatePlant(
            [Service] PlantService service,
            PlantInput input
         ) => await service.CreatePlant(input);

        public async Task<MutationResult<LotPartDTO>> CreateLotPartQuantityReceived(
            [Service] LotPartService service,
            ReceiveLotPartInput input
        ) => await service.CreateLotPartQuantityReceived(input);

        public async Task<MutationResult<DcwsResponse>> VerifyComponentSerial(
            [Service] VerifySerialService verifySerialService,
            Guid kitComponentId
        ) => await verifySerialService.VerifyComponentSerial(kitComponentId);

        public async Task<MutationResult<ReceiveHandlingUnitPayload>> SetHandlingUnitReceived(
            [Service] HandlingUnitService service,
            ReceiveHandlingUnitInput input
        ) => await service.SetHandlingUnitReceived(input);


        // tmp

        public record ApplyComponentSerialFormatInput(Guid Id);
        public async Task<ComponentSerial> ApplyComponentSerialFormat(
            [Service] ComponentSerialService service,
            ApplyComponentSerialFormatInput input
        ) => await service.ApplyComponentSerialFormat(input.Id);

        public async Task<MutationResult<Lot>> SetLotNote(
            [Service] BomService service,
            LotNoteInput input
        ) => await service.SetLotNote(input);

        public async Task<MutationResult<Kit>> SyncKfitModelComponents(
          [Service] PcvService service,
          string kitNo
        ) => await service.SyncKfitPcvComponents(kitNo);

        public async Task<MutationResult<List<KitSnapshot>>> RollbackKitsnapshots(
          [Service] KitSnapshotService service,
          string kitNo,
          TimeLineEventCode toTimelineEventCode
        ) => await service.RollbackKitSnapshots(kitNo, toTimelineEventCode);

        public async Task<MutationResult<PartnerStatusAck>> ImportPartnerStatusAck(
            [Service] KitSnapshotService service,
            PartnerStatusAckDTO input
        ) => await service.ImportPartnerStatusAck(input);
        
    }
}
