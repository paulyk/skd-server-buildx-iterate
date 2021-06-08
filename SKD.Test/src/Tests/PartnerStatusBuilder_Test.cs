using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Service.Util;

namespace SKD.Test {

    public class PartnerStatusBuilder_Test : TestBase {

        private string DEALEAR_CODE = "EN";
        private DateTime CUSTOM_RECEIVED_DATE = DateTime.UtcNow.AddDays(-15).Date;
        private DateTime PLAN_BUILD_DATE = DateTime.UtcNow.AddDays(-7).Date;
        private DateTime BUILD_COMPLETE_DATE = DateTime.UtcNow.AddDays(-6).Date;
        private DateTime GATE_RELEASE_DATE = DateTime.UtcNow.AddDays(-3).Date;
        private DateTime WHOLESALED_DATE = DateTime.UtcNow.AddDays(-2).Date;
        private string ENGINE_SERIAL = "GRBPA20346000023FB3Q 6007 AE3E";

        public PartnerStatusBuilder_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: true, assignVin: true);
        }

        [Fact]
        public async Task can_generate_partner_status_payload() {
            var snapshotRun = await GenerateKitSnapshotRun_TestData();

            var service = new PartnerStatusBuilder(context);
            var input = new PartnerStatusBuilder.Input {
                PlantCode = snapshotRun.Plant.Code,
                SnapshotDate = snapshotRun.RunDate
            };
            var paylaod = await service.BuildPartnerStatusPaylaod(input);

            var exptectedLines = 8;  // header + trailer + 6 kits
            var lines = paylaod.Payload.Split('\n');
            var actuaLineCount = lines.Count();
            // assert line count
            Assert.Equal(exptectedLines, actuaLineCount);
            // assert header
            var headerLine = new FlatFileLine(new PartnerStatusLayout.Header());
            var headerFields = headerLine.Parse(lines[0]);

            var expectedValue = PartnerStatusLayout.HDR_RECORD_TYPE_VAL;
            var actualField = headerFields.FirstOrDefault(t => t.Name == nameof(PartnerStatusLayout.Header.HDR_RECORD_TYPE));
            var actualValue = actualField != null ? actualField.Value : "";

            Assert.Equal(expectedValue, actualValue);

            expectedValue = PartnerStatusLayout.HDR_FILE_NAME_VAL;
            actualValue = headerFields.First(t => t.Name == nameof(PartnerStatusLayout.Header.HDR_FILE_NAME)).Value;
            Assert.Equal(expectedValue, actualValue);
        }

        private async Task<KitSnapshotRun> GenerateKitSnapshotRun_TestData() {
            var lot = await context.Lots.Include(t => t.Kits).FirstOrDefaultAsync();

            var kitSnapshotRun = new KitSnapshotRun {
                PlantId = lot.PlantId,
                RunDate = DateTime.UtcNow,
                Sequence = 1,
                KitSnapshots = lot.Kits.Select(kit => new KitSnapshot {
                    Kit = kit,
                    ChangeStatusCode = PartnerStatus_ChangeStatus.Added,
                    TimelineEventCode = TimeLineEventType.CUSTOM_RECEIVED,
                    VIN = kit.VIN,
                    DealerCode = DEALEAR_CODE,
                    EngineSerialNumber = ENGINE_SERIAL,
                    CustomReceived = CUSTOM_RECEIVED_DATE,
                    PlanBuild = PLAN_BUILD_DATE,
                    OrginalPlanBuild = (DateTime?)null,
                    BuildCompleted = BUILD_COMPLETE_DATE,
                    GateRelease = GATE_RELEASE_DATE,
                    Wholesale = WHOLESALED_DATE,
                }).ToList()
            };

            context.KitSnapshotRuns.Add(kitSnapshotRun);

            await context.SaveChangesAsync();     

            // kitSnapshotRun = await context.KitSnapshotRuns
            //     .Include(t => t.Plant)       
            //     .FirstOrDefaultAsync(t => t.Id == kitSnapshotRun.Id);

            return kitSnapshotRun;
        }
    }

}