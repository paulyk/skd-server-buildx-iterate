using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Common;

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
            var headerLineText = lines[0];
            // assert line count
            Assert.Equal(exptectedLines, actuaLineCount);

            // filename prefix
            var expecedPrefix = PartnerStatusLayout.FILENAME_PREFIX;
            var actualPrefix = payload.Filename.Substring(0, PartnerStatusLayout.FILENAME_PREFIX.Length);
            Assert.Equal(expecedPrefix, actualPrefix);


            // Header 
            var headerLine = new FlatFileLine<PartnerStatusLayout.Header>();
            var expected_HDR_REORD_TYPE = PartnerStatusLayout.HDR_RECORD_TYPE_VAL;
            var actual_HDR_REORD_TYPE = headerLine.GetFieldValue(headerLineText, t => t.HDR_RECORD_TYPE);
            Assert.Equal(expected_HDR_REORD_TYPE, actual_HDR_REORD_TYPE);

            // Header HDR_FILE_NAME_VAL
            var actual_HDR_FILE_NAME = headerLine.GetFieldValue(headerLineText, t => t.HDR_FILE_NAME).Trim();
            var expected_HDR_FILE_NAME = PartnerStatusLayout.HDR_FILE_NAME_VAL;
            Assert.Equal(expected_HDR_FILE_NAME, actual_HDR_FILE_NAME);

            // Detail  ENGINE
            var detailLine = new FlatFileLine<PartnerStatusLayout.Detail>();

            var detailLayout = new PartnerStatusLayout.Detail();
            var expected_EngineSerial = ENGINE_SERIAL.Substring(0, detailLayout.PST_ENGINE_SERIAL_NUMBER);
            var actual_EngineSerial =detailLine.GetFieldValue(lines[1], t => t.PST_ENGINE_SERIAL_NUMBER);
            Assert.Equal(expected_EngineSerial, actual_EngineSerial);
        }

        private async Task<KitSnapshotRun> GenerateKitSnapshotRun_TestData() {
            var lot = await context.Lots.Include(t => t.Kits).FirstOrDefaultAsync();
            var timelineEventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == TimeLineEventCode.CUSTOM_RECEIVED);

            var kitSnapshotRun = new KitSnapshotRun {
                PlantId = lot.PlantId,
                RunDate = DateTime.UtcNow.Date,
                Sequence = 1,
                KitSnapshots = lot.Kits.Select(kit => new KitSnapshot {
                    Kit = kit,
                    ChangeStatusCode = PartnerStatus_ChangeStatus.Added,
                    KitTimeLineEventType = timelineEventType,
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