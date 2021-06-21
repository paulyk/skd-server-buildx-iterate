#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SKD.Common;
using SKD.Model;
using SKD.Service;

namespace SKD.Service {
    public class PartnerStatusBuilder {

        private readonly SkdContext context;
        public PartnerStatusBuilder(SkdContext context) {
            this.context = context;
        }

        public async Task<PartnerStatusDTO> GeneratePartnerStatusFilePaylaod(string plantCode, int sequence) {

            var kitSnapshotRun = await context.KitSnapshotRuns
                .Include(t => t.Plant)                
                .Include(t => t.KitSnapshots.OrderBy(u => u.Kit.Lot.LotNo).ThenBy(u => u.Kit.KitNo)).ThenInclude(t => t.Kit).ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == plantCode && t.Sequence == sequence)
                .FirstOrDefaultAsync();

            if (kitSnapshotRun == null) {
                return new PartnerStatusDTO {
                    PlantCode = plantCode,
                    Sequecne = sequence,
                    RunDate = (DateTime?)null,
                    ErrorMessage = $"Kit snapshot not found for plant {plantCode} seq {sequence}",
                    PayloadText = ""
                };
            }

            var lines = new List<string>();

            // heder
            var headerLineBuilder = new FlatFileLine<PartnerStatusLayout.Header>();
            lines.Add(headerLineBuilder.Build(BuildHeaderFields(kitSnapshotRun)));

            // detail
            var detialLine = new FlatFileLine<PartnerStatusLayout.Detail>();
            foreach (var snapshot in kitSnapshotRun.KitSnapshots) {
                var detailFields = BuildDetailFields(snapshot);
                var line = detialLine.Build(detailFields);
                lines.Add(line);
            }

            // trailer
            var trailerLine = new FlatFileLine<PartnerStatusLayout.Trailer>();
            var trailerFields = BuildTrailerFields(kitSnapshotRun);
            lines.Add(trailerLine.Build(trailerFields));

            var payload = new PartnerStatusDTO {
                PlantCode = kitSnapshotRun.Plant.Code,
                Sequecne = kitSnapshotRun.Sequence,
                RunDate = kitSnapshotRun.RunDate,
                Filename =  BuildFilename(kitSnapshotRun.Plant.Code, kitSnapshotRun.Plant.PartnerPlantCode, kitSnapshotRun.RunDate),
                PayloadText = String.Join('\n', lines)
            };
            return payload;
        }

        public string BuildFilename( string plantCode, string partnerPlantCode, DateTime runDate) {
            var formattedRunDate = runDate.ToString(PartnerStatusLayout.FILENAME_DATE_FORMAT);
            var prefix = PartnerStatusLayout.FILENAME_PREFIX;            
            return $"{prefix}_{plantCode}_{partnerPlantCode}_{formattedRunDate}.txt";
        }
        public List<FlatFileLine<PartnerStatusLayout.Header>.FieldValue> BuildHeaderFields(KitSnapshotRun snapshotRun) {
            var headerLayout = new PartnerStatusLayout.Header();
            var headerLineBuilder = new FlatFileLine<PartnerStatusLayout.Header>();

            return new List<FlatFileLine<PartnerStatusLayout.Header>.FieldValue> {
                headerLineBuilder.CreateFieldValue(t => t.HDR_RECORD_TYPE, PartnerStatusLayout.HDR_RECORD_TYPE_VAL),
                headerLineBuilder.CreateFieldValue(t => t.HDR_FILE_NAME, PartnerStatusLayout.HDR_FILE_NAME_VAL),                
                headerLineBuilder.CreateFieldValue(t => t.HDR_KD_PLANT_GSDB, snapshotRun.Plant.Code),                
                headerLineBuilder.CreateFieldValue(t => t.HDR_PARTNER_GSDB,snapshotRun.Plant.PartnerPlantCode),
                headerLineBuilder.CreateFieldValue(t => t.HDR_PARTNER_TYPE, snapshotRun.Plant.PartnerPlantType),            
                headerLineBuilder.CreateFieldValue(
                    t => t.HDR_SEQ_NBR,
                    snapshotRun.Sequence.ToString().PadLeft(headerLayout.HDR_SEQ_NBR,'0')),                
                headerLineBuilder.CreateFieldValue(
                    t => t.HDR_BATCH_DATE,
                    snapshotRun.RunDate.ToString(PartnerStatusLayout.HDR_BATCH_DATE_FORMAT)),
                headerLineBuilder.CreateFieldValue(
                    t => t.HDR_FILLER, new String(' ', headerLayout.HDR_FILLER)),                
            };
        }

        public List<FlatFileLine<PartnerStatusLayout.Detail>.FieldValue> BuildDetailFields(KitSnapshot snapshot) {
            var layout = new PartnerStatusLayout.Detail();
            var lineBuilder = new FlatFileLine<PartnerStatusLayout.Detail>();

            var detailFields =  new List<FlatFileLine<PartnerStatusLayout.Detail>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.PST_RECORD_TYPE, PartnerStatusLayout.PST_RECORD_TYPE_VAL),
                lineBuilder.CreateFieldValue(t => t.PST_TRAN_TYPE, snapshot.ChangeStatusCode.ToString()),
                lineBuilder.CreateFieldValue(t => t.PST_LOT_NUMBER, snapshot.Kit.Lot.LotNo),
                lineBuilder.CreateFieldValue(t => t.PST_KIT_NUMBER,snapshot.Kit.KitNo),
                lineBuilder.CreateFieldValue(t => t.PST_PHYSICAL_VIN, snapshot.VIN),

                lineBuilder.CreateFieldValue(
                    t => t.PST_BUILD_DATE,
                    snapshot.OrginalPlanBuild != null
                        ? snapshot.OrginalPlanBuild.Value.ToString(PartnerStatusLayout.PST_DATE_FORMAT)
                        : ""
                ),

                lineBuilder.CreateFieldValue(
                    t => t.PST_ACTUAL_DEALER_CODE,
                    snapshot.DealerCode != null ? snapshot.DealerCode : ""
                ),
                lineBuilder.CreateFieldValue(
                    t => t.PST_ENGINE_SERIAL_NUMBER,
                    snapshot.EngineSerialNumber
                ),
                lineBuilder.CreateFieldValue(
                    t => t.PST_CURRENT_STATUS,
                    ToFordTimelineCode(snapshot.KitTimeLineEventType.Code)
                ),

                lineBuilder.CreateFieldValue(t => t.PST_IP1R_STATUS_DATE, ""),
                lineBuilder.CreateFieldValue(t => t.PST_IP1S_STATUS_DATE, ""),
                lineBuilder.CreateFieldValue(t => t.PST_IP2R_STATUS_DATE, ""),
                lineBuilder.CreateFieldValue(t => t.PST_IP2S_STATUS_DATE,""),

                lineBuilder.CreateFieldValue(
                    t => t.PST_FPRE_STATUS_DATE,
                    FormattedDate(snapshot.CustomReceived, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                lineBuilder.CreateFieldValue(
                    t => t.PST_FPBP_STATUS_DATE,
                    FormattedDate(snapshot.PlanBuild, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                lineBuilder.CreateFieldValue(
                    t => t.PST_FPBC_STATUS_DATE,
                    FormattedDate(snapshot.BuildCompleted, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                lineBuilder.CreateFieldValue(
                    t => t.PST_FPGR_STATUS_DATE,
                        FormattedDate(snapshot.GateRelease, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                lineBuilder.CreateFieldValue(
                    t => t.PST_FPWS_STATUS_DATE,
                        FormattedDate(snapshot.Wholesale, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                lineBuilder.CreateFieldValue(t => t.PST_FILLER, "")
            };

            return detailFields;
        }

        public List<FlatFileLine<PartnerStatusLayout.Trailer>.FieldValue> BuildTrailerFields(KitSnapshotRun snapshotRun) {
            var layout = new PartnerStatusLayout.Trailer();
            var lineBuilder = new FlatFileLine<PartnerStatusLayout.Trailer>();

            return new List<FlatFileLine<PartnerStatusLayout.Trailer>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.TLR_RECORD_TYPE, "TLR"),
                lineBuilder.CreateFieldValue(t => t.TLR_FILE_NAME, "PARTNER_STATUS"),
                lineBuilder.CreateFieldValue(t => t.TLR_KD_PLANT_GSDB, snapshotRun.Plant.Code),
                lineBuilder.CreateFieldValue(t => t.TLR_PARTNER_GSDB, snapshotRun.Plant.PartnerPlantCode),
                lineBuilder.CreateFieldValue(
                    t => t.TLR_TOTAL_RECORDS,
                    // add 2 for Header + Trailer
                    (snapshotRun.KitSnapshots.Count + 2).ToString().PadLeft(layout.TLR_TOTAL_RECORDS, '0')),
                lineBuilder.CreateFieldValue(t => t.TLR_FILLER, ""),
            };
        }

        public string ToFordTimelineCode(TimeLineEventCode timeLineEventType) {
            switch (timeLineEventType) {
                case TimeLineEventCode.CUSTOM_RECEIVED: return   FordTimeLineCode.FPCR.ToString(); //"FPCR";
                case TimeLineEventCode.PLAN_BUILD: return FordTimeLineCode.FPBP.ToString(); //"FPBP";
                case TimeLineEventCode.BUILD_COMPLETED: return FordTimeLineCode.FPBC.ToString(); //"FPBC";
                case TimeLineEventCode.GATE_RELEASED: return FordTimeLineCode.FPGR.ToString(); //"FPGR";
                case TimeLineEventCode.WHOLE_SALE: return  FordTimeLineCode.FPWS.ToString(); //"FPWS";
                default: throw new Exception("Unexpected timlien event " + timeLineEventType);
            }
        }

        private string FormattedDate(DateTime? date, string dateFormat) {
            return date.HasValue
                ? date.Value.ToString(dateFormat)
                : "";
        }

            
    }
}