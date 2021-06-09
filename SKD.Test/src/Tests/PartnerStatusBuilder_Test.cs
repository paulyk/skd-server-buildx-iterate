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

        private string DEALEAR_CODE = "DLR13";
        private DateTime CUSTOM_RECEIVED_DATE = DateTime.UtcNow.AddDays(-15).Date;
        private DateTime PLAN_BUILD_DATE = DateTime.UtcNow.AddDays(-7).Date;
        private DateTime BUILD_COMPLETE_DATE = DateTime.UtcNow.AddDays(-6).Date;
        private DateTime GATE_RELEASE_DATE = DateTime.UtcNow.AddDays(-3).Date;
        private DateTime WHOLESALED_DATE = DateTime.UtcNow.AddDays(-2).Date;
        private string ENGINE_SERIAL = "GRBPA20346000023FB3Q 6007 AE3E MORE THAN";

        public PartnerStatusBuilder_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: true, assignVin: true);
        }

        [Fact]
        public async Task can_generate_partner_status_file_payload() {
            var snapshotRun = await GenerateKitSnapshotRun_TestData();

            var service = new PartnerStatusBuilder(context);
            var payload = await service.GeneratePartnerStatusFilePaylaod(
                plantCode: snapshotRun.Plant.Code,
                sequence: snapshotRun.Sequence
            );

            var exptectedLines = 8;  // header + trailer + 6 kits
            var lines = payload.PayloadText.Split('\n');
            var actuaLineCount = lines.Count();
            // assert line count
            Assert.Equal(exptectedLines, actuaLineCount);

            // filename prefix
            var expecedPrefix = PartnerStatusLayout.FILENAME_PREFIX;
            var actualPrefix = payload.Filename.Substring(0, PartnerStatusLayout.FILENAME_PREFIX.Length);
            Assert.Equal(expecedPrefix, actualPrefix);

            // assert header
            var headerLine = new FlatFileLine(new PartnerStatusLayout.Header());
            var headerFields = headerLine.Parse(lines[0]);

            // Header 
            var expectedValue = PartnerStatusLayout.HDR_RECORD_TYPE_VAL;
            var actualField = headerFields.FirstOrDefault(t => t.Name == nameof(PartnerStatusLayout.Header.HDR_RECORD_TYPE));
            var actualValue = actualField != null ? actualField.Value : "";
            Assert.Equal(expectedValue, actualValue);

            // Header HDR_FILE_NAME_VAL
            expectedValue = PartnerStatusLayout.HDR_FILE_NAME_VAL;
            actualValue = headerFields.First(t => t.Name == nameof(PartnerStatusLayout.Header.HDR_FILE_NAME)).Value.Trim();
            Assert.Equal(expectedValue, actualValue);

            // Detail  ENGINE
            var detailLine = new FlatFileLine(new PartnerStatusLayout.Detail());
            var engineField = detailLine.Fields.First(t => t.Name == nameof(PartnerStatusLayout.Detail.PST_ENGINE_SERIAL_NUMBER));
            var fields = detailLine.Parse(lines[1]);
            var engineSerialField = fields.First(t => t.Name == nameof(PartnerStatusLayout.Detail.PST_ENGINE_SERIAL_NUMBER));
            Assert.Equal(ENGINE_SERIAL.Substring(0, engineField.Length), engineSerialField.Value);
        }

        private async Task<KitSnapshotRun> GenerateKitSnapshotRun_TestData() {
            var lot = await context.Lots.Include(t => t.Kits).FirstOrDefaultAsync();

            var kitSnapshotRun = new KitSnapshotRun {
                PlantId = lot.PlantId,
                RunDate = DateTime.UtcNow.Date,
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
            return kitSnapshotRun;
        }
    }
}