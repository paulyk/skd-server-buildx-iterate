using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Common;
using System.Text.RegularExpressions;

namespace SKD.Test {

    public class PartnerStatusBuilder_Test : TestBase {

        private readonly string DEALEAR_CODE = "DLR13";
        private readonly DateTime CUSTOM_RECEIVED_DATE = DateTime.UtcNow.AddDays(-15).Date;
        private readonly DateTime PLAN_BUILD_DATE = DateTime.UtcNow.AddDays(-7).Date;
        private readonly DateTime BUILD_COMPLETE_DATE = DateTime.UtcNow.AddDays(-6).Date;
        private readonly DateTime GATE_RELEASE_DATE = DateTime.UtcNow.AddDays(-3).Date;
        private readonly DateTime WHOLESALED_DATE = DateTime.UtcNow.AddDays(-2).Date;
        private readonly string ENGINE_SERIAL = "GRBPA20346000023FB3Q 6007 AE3E MORE THAN";

        public PartnerStatusBuilder_Test() {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data(generateLot: true, assignVin: true);
        }

        [Fact]
        public async Task Can_generate_partner_status_file_payload() {
            var result = await GenerateKitSnapshotRun_TestData();
            var snapshotRun = await context.KitSnapshotRuns
                .Include(t => t.Plant)
                .FirstOrDefaultAsync(t => t.Id == result.Id);


            var service = new PartnerStatusBuilder(context);
            var payload = await service.GeneratePartnerStatusFilePaylaod(
                plantCode: snapshotRun.Plant.Code,
                sequence: snapshotRun.Sequence
            );

            var exptectedLines = 8;  // header + trailer + 6 kits
            var lines = payload.PayloadText.Split('\n');
            var actuaLineCount = lines.Length;
            var headerLineText = lines[0];
            var trailerLineText = lines[^1];
            // assert line count
            Assert.Equal(exptectedLines, actuaLineCount);

            // filename prefix
            var expecedPrefix = PartnerStatusLayout.FILENAME_PREFIX;
            var actualPrefix = payload.Filename.Substring(0, PartnerStatusLayout.FILENAME_PREFIX.Length);
            Assert.Equal(expecedPrefix, actualPrefix);

            AssertHeader();
            AssertDetial();
            AssertTrailer();
            await AssertFilename();

            async Task AssertFilename() {
                var filename = await service.GenPartnerStatusFilename(snapshotRun.Id);
                var runDate = snapshotRun.RunDate.ToString(PartnerStatusLayout.FILENAME_DATE_FORMAT);
                var pattern = $"{PartnerStatusLayout.FILENAME_PREFIX}_{snapshotRun.Plant.Code}_{snapshotRun.Plant.PartnerPlantCode}_{runDate}.txt";
                var regex = new Regex(pattern);
                var match = regex.Match(filename);
                Assert.True(match.Success);
            }            

            void AssertHeader() {
                // Header 
                var headerLineParser = new FlatFileLine<PartnerStatusLayout.Header>();
                var headerLayout = new PartnerStatusLayout.Header();
                // Header HDR_RECORD_TYPE_VAL
                var actual_HDR_REORD_TYPE = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_RECORD_TYPE);
                Assert.Equal(PartnerStatusLayout.Header.HDR_RECORD_TYPE_VAL, actual_HDR_REORD_TYPE);

                // Header HDR_FILE_NAME_VAL
                var actual_HDR_FILE_NAME = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_FILE_NAME).Trim();
                Assert.Equal(PartnerStatusLayout.Header.HDR_FILE_NAME_VAL, actual_HDR_FILE_NAME);

                // Header HDR_KD_PLANT_GSDB
                var actual_HDR_KD_PLANT_GSDB = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_KD_PLANT_GSDB);
                Assert.Equal(snapshotRun.Plant.Code, actual_HDR_KD_PLANT_GSDB);

                // HDR_PARTNER_GSDB
                var actual_HDR_PARTNER_GSDB = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_PARTNER_GSDB);
                Assert.Equal(snapshotRun.Plant.PartnerPlantCode, actual_HDR_PARTNER_GSDB);

                // HDR_PARTNER_TYPE 
                var actual_DR_PARTNER_TYPE = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_PARTNER_TYPE).Trim();
                Assert.Equal(snapshotRun.Plant.PartnerPlantType, actual_DR_PARTNER_TYPE);

                // HDR_SEQ_NBR
                var actual_HDR_SEQ_NBR = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_SEQ_NBR);
                Assert.Equal(snapshotRun.Sequence.ToString().PadLeft(headerLayout.HDR_SEQ_NBR, '0'), actual_HDR_SEQ_NBR);

                // HDR_BATCH_DATE
                var actual_HDR_BATCH_DATE = headerLineParser.GetFieldValue(headerLineText, t => t.HDR_BATCH_DATE).Trim();
                Assert.Equal(snapshotRun.RunDate.ToString(PartnerStatusLayout.Header.HDR_BATCH_DATE_FORMAT), actual_HDR_BATCH_DATE);

            }

            void AssertDetial() {
                var detailLineParser = new FlatFileLine<PartnerStatusLayout.Detail>();
                var detailLayout = new PartnerStatusLayout.Detail();

                var firstDetailLine = lines[1];
                var firstKitSnapshot = snapshotRun.KitSnapshots.First();

                // PST_RECORD_TYPE
                var actual_PST_RECORD_TYPE = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_RECORD_TYPE);
                Assert.Equal(PartnerStatusLayout.PST_RECORD_TYPE_VAL, actual_PST_RECORD_TYPE);

                // PST_TRAN_TYPE
                var actual_PST_TRAN_TYPE = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_TRAN_TYPE);
                Assert.Equal(firstKitSnapshot.ChangeStatusCode.ToString()[0].ToString(), actual_PST_TRAN_TYPE);

                // PST_LOT_NUMBER
                var actual_PST_LOT_NUMBER = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_LOT_NUMBER);
                Assert.Equal(firstKitSnapshot.Kit.Lot.LotNo, actual_PST_LOT_NUMBER);

                // PST_KIT_NUMBER
                var actual_PST_KIT_NUMBER = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_KIT_NUMBER);
                Assert.Equal(firstKitSnapshot.Kit.KitNo, actual_PST_KIT_NUMBER);

                // PST_CURRENT_STATUS
                var actual_PST_CURRENT_STATUS = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_CURRENT_STATUS);
                Assert.Equal(service.ToFordTimelineCode(firstKitSnapshot.KitTimeLineEventType.Code), actual_PST_CURRENT_STATUS);

                // PST_ENGINE_SERIAL_NUMBER
                var expected_EngineSerial = ENGINE_SERIAL.Substring(0, detailLayout.PST_ENGINE_SERIAL_NUMBER);
                var actual_EngineSerial = detailLineParser.GetFieldValue(lines[1], t => t.PST_ENGINE_SERIAL_NUMBER);
                Assert.Equal(expected_EngineSerial, actual_EngineSerial);

                // PST_FPRE_STATUS_DATE // custom receive
                var actual_PST_FPRE_STATUS_DATE = detailLineParser.GetFieldValue(firstDetailLine, t => t.PST_FPRE_STATUS_DATE).Trim();
                var expected_PST_FPRE_STATUS_DATE = firstKitSnapshot.CustomReceived.HasValue
                    ? firstKitSnapshot.CustomReceived.Value.ToString(PartnerStatusLayout.PST_STATUS_DATE_FORMAT).Trim()
                    : "";
                Assert.Equal(expected_PST_FPRE_STATUS_DATE, actual_PST_FPRE_STATUS_DATE);
            }

            void AssertTrailer() {
                var trailerLineParser = new FlatFileLine<PartnerStatusLayout.Trailer>();
                var detailLayout = new PartnerStatusLayout.Trailer();
                var trailerLineText = lines[^1];

                // HDR_PARTNER_TYPE 
                var actual_TLR_RECORD_TYPE = trailerLineParser.GetFieldValue(trailerLineText, t => t.TLR_RECORD_TYPE).Trim();
                Assert.Equal(PartnerStatusLayout.TLR_RECORD_TYPE_VAL, actual_TLR_RECORD_TYPE);

                var actual_TLR_FILE_NAME = trailerLineParser.GetFieldValue(trailerLineText, t => t.TLR_FILE_NAME).Trim();
                Assert.Equal(PartnerStatusLayout.TLR_FILE_NAME_VAL, actual_TLR_FILE_NAME);

                var actual_TLR_KD_PLANT_GSDB = trailerLineParser.GetFieldValue(trailerLineText, t => t.TLR_KD_PLANT_GSDB).Trim();
                Assert.Equal(snapshotRun.Plant.Code, actual_TLR_KD_PLANT_GSDB);

                var actual_TLR_PARTNER_GSDB = trailerLineParser.GetFieldValue(trailerLineText, t => t.TLR_PARTNER_GSDB).Trim();
                Assert.Equal(snapshotRun.Plant.PartnerPlantCode, actual_TLR_PARTNER_GSDB);

                var actual_TLR_TOTAL_RECORDS = int.Parse(trailerLineParser.GetFieldValue(trailerLineText, t => t.TLR_TOTAL_RECORDS));
                Assert.Equal(lines.Length, actual_TLR_TOTAL_RECORDS);

            }
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