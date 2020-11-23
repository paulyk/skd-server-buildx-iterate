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
            await Gen_PartStatus_Test_Data(plantCode);

            // services
            var service = new PartnerStatusService(ctx);
            var vehicleService = new VehicleService(ctx);

            // day 1
            var date = new DateTime(2020, 12, 1);
            var vehicleEntries = await GetVehicleStatusEntries(plantCode, date);
            var entryCount = vehicleEntries.Count;
            Assert.Equal(0, entryCount);

            // day 2
            date = date.AddDays(1);
            var vehicle = ctx.Vehicles.First();
            await AddVehicleTimelineEntry(TimeLineEventType.CUSTOM_RECEIVED, vehicle.KitNo, date, date);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, date);

            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            var entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.Added, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPCR, entry.CurrentStatusType);


            // day 3   (no changes)
            date = date.AddDays(1);
            vehicleEntries = await GetVehicleStatusEntries(plantCode, date);
            entryCount = vehicleEntries.Count;
            Assert.Equal(1, entryCount);
            entry = vehicleEntries.First();
            Assert.Equal(PartnerStatus_TxType.NoChange, entry.TxType);
            Assert.Equal(PartnerStatus_CurrentStatusType.FPCR, entry.CurrentStatusType);





            // 
            //  1  vehicle lot imort   Assert; PS = 0
            //  2   custom received    Assert: PS = 1,  Tx_Type = A
            //  3                      Assert: PS = 1,  Tx_Type = N
            //  4   custom received    Assert: PS = 2,  Tx_Type = A, N
            //  5   plant build        Assert: PS = 2,  Tx_Type = C, N
            //  6   wholesale          Assert: PS = 2,  Tx_Type = F, N
            //  7                      Assert: PS = 2,  Tx_Type = F, N
            // 17                      Assert: PS = 1,  Tx_Type = N



        }


        private async Task<List<PartnerStatusDTO.VehicleStatus>> GetVehicleStatusEntries(string plantCode, DateTime date) {
            var partnerStatusInput = new PartnerStatusInput {
                PlantCode = plantCode,
                RunDate = date
            };

            // initial
            var service = new PartnerStatusService(ctx);
            var payload = await service.GetPartnerStatus(partnerStatusInput);
            return payload.Entity.VehicleStatusEntries.ToList();
        }
        private async Task AddVehicleTimelineEntry(TimeLineEventType eventType, string kitNo, DateTime eventDate, DateTime createdAd) {
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleTimelineEvent(new VehicleTimelineEventInput {
                KitNo = kitNo,
                EventType = eventType,
                EventDate = eventDate
            });
            SetEntityCreatedAt<VehicleTimelineEvent>(ctx, payload.Entity.Id, createdAd);
        }

        private async Task Gen_PartStatus_Test_Data(string plantCode) {
            Gen_VehicleTimelineEventTypes(ctx);

            var lotNo = Gen_LotNo();
            var modelCode = Gen_VehicleModel_Code();
            var vehicleLot = await Gen_Vehicle_Lot(
                ctx, lotNo, plantCode,
                modelCode, new List<(string, string)> { ("AB", "stating_1"), ("EN", "station_2") });

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

    }
}