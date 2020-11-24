using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ParnterStatusServiceTest : TestBase {

        private SkdContext ctx;
        public ParnterStatusServiceTest() {
            ctx = GetAppDbContext();
        }

        [Fact]
        public async Task can_generate_partner_status() {
            // setup

            var plantCode = Gen_PlantCode();
            var engineComponentCode = "EN";
            var engineSerial = "SN777777";
            var dealerCode = "DLR77777";
            // generate vehicle lot with 6 kits (vehicles)
            await Gen_PartStatus_Test_Data(plantCode, engineComponentCode);

            // services
            var service = new PartnerStatusService(ctx);
            var vehicleService = new VehicleService(ctx);

            // day 1  (no timeline events)
            var date = new DateTime(2020, 12, 1);
            var vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            var entryCount = vehicleEntries.Count;
            Assert.Equal(0, entryCount);

            // day 2  (Custom Received)
            date = date.AddDays(1);
            var vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);

            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            var entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Added, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPCR, entry.CurrentStatusType);

            // day 3   (no change)
            date = date.AddDays(1);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.NoChange, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPCR, entry.CurrentStatusType);

            // day 3   (PLan Build)
            date = date.AddDays(1);
            await AddVehicleTimelineEntry(TimeLineEventType.PLAN_BUILD, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPBP, entry.CurrentStatusType);

            // day 5   (no change)
            date = date.AddDays(1);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.NoChange, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPBP, entry.CurrentStatusType);

            // day 6   (Build Completed)
            // Scan engine serial number 
            // DCWS Response accepting engine serial number
            // Add Build Completed timeline event
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddEngineSerialNumberComponentScan(vehicle.KitNo, engineComponentCode, engineSerial);
            await AddVehicleTimelineEntry(TimeLineEventType.BULD_COMPLETED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPBC, entry.CurrentStatusType);
            Assert.Equal(engineSerial, entry.EngineSerialNumber);

            // day 6   (Gate Release)
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.GATE_RELEASED, vehicle.KitNo, "", date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Changed, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPGR, entry.CurrentStatusType);

            // day 7  (Wholesale)
            date = date.AddDays(1);
            vehicle = ctx.Vehicles.OrderBy(t => t.KitNo).First();
            await AddVehicleTimelineEntry(TimeLineEventType.WHOLE_SALE, vehicle.KitNo, dealerCode, date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Final, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPWS, entry.CurrentStatusType);
            Assert.Equal(dealerCode, entry.DealerCode);

            // day 8   (final)
            date = date.AddDays(1);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, engineComponentCode, date);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Final, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPWS, entry.CurrentStatusType);

        }

        #region test helper methods
        private async Task<List<PartnerStatusDTO.VehicleStatus>> GetVehicleStatusEntries(string plantCode, string engineComponentCode, DateTime date) {
            var partnerStatusInput = new PartnerStatusInput {
                PlantCode = plantCode,
                EngineComponentCode = engineComponentCode,
                RunDate = date
            };

            // initial
            var service = new PartnerStatusService(ctx);
            var payload = await service.GetPartnerStatus(partnerStatusInput);
            return payload.Entity.VehicleStatusEntries.ToList();
        }

        private async Task AddEngineSerialNumberComponentScan(string kitNo, string engineComponentCode, string engineSerial) {
            var engineVehicleComponent = ctx.VehicleComponents
                .First(t => t.Vehicle.KitNo == kitNo && t.Component.Code == engineComponentCode);
            var scanService = new ComponentScanService(ctx);
            var createScanPayload = await scanService.CreateComponentScan(new ComponentScanInput {
                VehicleComponentId = engineVehicleComponent.Id,
                Scan1 = engineSerial,
                Scan2 = ""
            });

            var componentScan = createScanPayload.Entity;
            var dcwsService = new DCWSResponseService(ctx);
            await dcwsService.CreateDCWSResponse(new DCWWResponseInput {
                ComponentScanId = componentScan.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            });
        }
        private async Task AddVehicleTimelineEntry(TimeLineEventType eventType, string kitNo, string eventNote, DateTime eventDate, DateTime createdAt) {
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleTimelineEvent(new VehicleTimelineEventInput {
                KitNo = kitNo,
                EventType = eventType,
                EventNote = eventNote,
                EventDate = eventDate
            });
            SetEntityCreatedAt<VehicleTimelineEvent>(ctx, payload.Entity.Id, createdAt);
        }

        private async Task Gen_PartStatus_Test_Data(string plantCode, string engineComponentCode) {
            Gen_VehicleTimelineEventTypes(ctx);

            var lotNo = Gen_LotNo();
            var modelCode = Gen_VehicleModel_Code();
            var vehicleLot = await Gen_Vehicle_Lot(
                ctx, lotNo, plantCode,
                modelCode, new List<(string, string)> { (engineComponentCode, "stating_1"), ("AB", "station_2") });

            // assert setup correct
            var vehicles = await ctx.Vehicles.AsNoTracking().ToListAsync();
            var vehicle_count = vehicles.Count();
        }

        public async Task<VehicleLot> Gen_Vehicle_Lot(
            SkdContext ctx,
            string lotNo,
            string plantCode,
            string modelCode,
            List<(string, string)> component_stations
        ) {
            var model = Gen_VehicleModel(
                ctx,
                modelCode,
                component_stations
            );
            var kitNos = Enumerable.Range(1, 6).Select(n => Gen_KitNo()).ToList();

            var input = Gen_VehicleLot_Input(lotNo, plantCode, modelCode, kitNos);

            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLot(input);

            return payload.Entity;
        }
        #endregion

    }
}