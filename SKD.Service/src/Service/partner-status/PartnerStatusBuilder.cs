#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SKD.Model;
using SKD.Service.Util;

namespace SKD.Service {
    public class PartnerStatusBuilder {

        private readonly SkdContext context;
        public PartnerStatusBuilder(SkdContext context) {
            this.context = context;
        }

        public class Input {
            public string PlantCode { get; set; } = "";
            public DateTime SnapshotDate { get; set; }
        }
        public async Task<MutationPayload<string>> BuildPartnerStatusPaylaod(Input input) {
            var payload = new MutationPayload<string>(null);
            payload.Errors = await ValidateBuildPartnerStatusInput(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            var ksr = await context.KitSnapshotRuns
                .Include(t => t.Plant)
                .Include(t => t.KitSnapshots).ThenInclude(t => t.Kit).ThenInclude(t => t.Lot)
                .FirstOrDefaultAsync(t => t.RunDate == input.SnapshotDate);


            var lines = new List<string>();

            // heder
            var headerLine = new FlatFileLine(new PartnerStatusLayout.Header());
            lines.Add(headerLine.Build(BuildHeaderFields(ksr)));

            // detail
            var detialLine = new FlatFileLine(new PartnerStatusLayout.Detail());
            foreach (var snapshot in ksr.KitSnapshots) {
                var detailFields = BuildDetailFields(snapshot);
                var line = detialLine.Build(detailFields);
                lines.Add(line);
            }

            // trailer
            var trailerLine = new FlatFileLine(new PartnerStatusLayout.Trailer());
            var trailerFields = BuildTrailerFields(ksr);
            lines.Add(trailerLine.Build(trailerFields));

            payload.Entity = String.Join('\n', lines);

            return payload;
        }

        public List<FlatFileLine.FieldValue> BuildHeaderFields(KitSnapshotRun snapshotRun) {
            var headerLayout = new PartnerStatusLayout.Header();

            return new List<FlatFileLine.FieldValue> {
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_RECORD_TYPE),
                    Value = PartnerStatusLayout.HDR_RECORD_TYPE_VAL
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_FILE_NAME),
                    Value = PartnerStatusLayout.HDR_FILE_NAME_VAL
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_KD_PLANT_GSDB),
                    Value = snapshotRun.Plant.Code
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_PARTNER_GSDB),
                    Value = snapshotRun.Plant.PartnerPlantCode
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_PARTNER_TYPE),
                    Value = snapshotRun.Plant.PartnerPlantType
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_SEQ_NBR),
                    Value = snapshotRun.Sequence.ToString().PadLeft(headerLayout.HDR_SEQ_NBR,'0')
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_BATCH_DATE),
                    Value = snapshotRun.RunDate.ToString(PartnerStatusLayout.HDR_BATCH_DATE_FORMAT)
                },
                new FlatFileLine.FieldValue {
                    Name = nameof(headerLayout.HDR_FILLER),
                    Value = new String(' ', headerLayout.HDR_FILLER)
                }
            };

        }

        public List<FlatFileLine.FieldValue> BuildDetailFields(KitSnapshot snapshot) {
            var layout = new PartnerStatusLayout.Detail();

            return new List<FlatFileLine.FieldValue> {
                new FlatFileLine.FieldValue( nameof(layout.PST_RECORD_TYPE),PartnerStatusLayout.PST_RECORD_TYPE_VAL),
                new FlatFileLine.FieldValue(nameof(layout.PST_TRAN_TYPE),snapshot.ChangeStatusCode.ToString().Substring(0,1)),
                new FlatFileLine.FieldValue(nameof(layout.PST_LOT_NUMBER), snapshot.Kit.Lot.LotNo),
                new FlatFileLine.FieldValue(nameof(layout.PST_KIT_NUMBER),snapshot.Kit.KitNo),
                new FlatFileLine.FieldValue(nameof(layout.PST_PHYSICAL_VIN), snapshot.Kit.VIN),

                new FlatFileLine.FieldValue(
                    nameof(layout.PST_BUILD_DATE),
                    snapshot.OrginalPlanBuild != null
                        ? snapshot.OrginalPlanBuild.Value.ToString(PartnerStatusLayout.PST_DATE_FORMAT)
                        : ""
                ),

                new FlatFileLine.FieldValue {
                    Name = nameof(layout.PST_ACTUAL_DEALER_CODE),
                    Value = snapshot.DealerCode != null ? snapshot.DealerCode : ""
                },
                new FlatFileLine.FieldValue(
                    nameof(layout.PST_ENGINE_SERIAL_NUMBER),
                    snapshot.EngineSerialNumber.Substring(0, layout.PST_ENGINE_SERIAL_NUMBER)
                ),
                new FlatFileLine.FieldValue(
                    nameof(layout.PST_CURRENT_STATUS),
                    ToFordTimelineCode(snapshot.TimelineEventCode)
                ),

                new FlatFileLine.FieldValue( nameof(layout.PST_IP1R_STATUS_DATE), ""),
                new FlatFileLine.FieldValue( nameof(layout.PST_IP1S_STATUS_DATE), ""),
                new FlatFileLine.FieldValue( nameof(layout.PST_IP2R_STATUS_DATE), ""),
                new FlatFileLine.FieldValue( nameof(layout.PST_IP2S_STATUS_DATE),""),

                new FlatFileLine.FieldValue(
                        nameof(layout.PST_FPRE_STATUS_DATE),
                        FormattedDate(snapshot.CustomReceived, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                new FlatFileLine.FieldValue(
                        nameof(layout.PST_FPBP_STATUS_DATE),
                        FormattedDate(snapshot.PlanBuild, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                new FlatFileLine.FieldValue(
                        nameof(layout.PST_FPBC_STATUS_DATE),
                        FormattedDate(snapshot.BuildCompleted, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                new FlatFileLine.FieldValue(
                        nameof(layout.PST_FPGR_STATUS_DATE),
                        FormattedDate(snapshot.GateRelease, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                new FlatFileLine.FieldValue(
                        nameof(layout.PST_FPWS_STATUS_DATE),
                        FormattedDate(snapshot.Wholesale, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                new FlatFileLine.FieldValue(nameof(layout.PST_FILLER), "")
            };
        }

        public List<FlatFileLine.FieldValue> BuildTrailerFields(KitSnapshotRun snapshotRun) {
            var layout = new PartnerStatusLayout.Trailer();

            return new List<FlatFileLine.FieldValue> {
                new FlatFileLine.FieldValue(nameof(layout.TLR_RECORD_TYPE), "TLR"),
                new FlatFileLine.FieldValue(nameof(layout.TLR_FILE_NAME), "PARTNER_STATUS"),
                new FlatFileLine.FieldValue(nameof(layout.TLR_KD_PLANT_GSDB), snapshotRun.Plant.Code),
                new FlatFileLine.FieldValue(nameof(layout.TLR_PARTNER_GSDB), snapshotRun.Plant.PartnerPlantCode),
                new FlatFileLine.FieldValue(
                    nameof(layout.TLR_TOTAL_RECORDS),
                    snapshotRun.KitSnapshots.Count.ToString().PadLeft(layout.TLR_TOTAL_RECORDS, '0')),
                new FlatFileLine.FieldValue(nameof(layout.TLR_FILLER), ""),
            };
        }

        public async Task<List<Error>> ValidateBuildPartnerStatusInput(Input input) {
            var errors = new List<Error>();
            var exits = await context.KitSnapshotRuns
                .Where(t => t.Plant.Code == input.PlantCode)
                .Where(t => t.RunDate == input.SnapshotDate)
                .AnyAsync();
            if (!exits) {
                errors.Add(new Error("", $"kit snaphot not found for date: {input.SnapshotDate.ToString("yyyy-mm-dd hh:mm")}"));
            }
            return errors;
        }

        public string ToFordTimelineCode(TimeLineEventType timeLineEventType) {
            switch (timeLineEventType) {
                case TimeLineEventType.CUSTOM_RECEIVED: return "FPCR";
                case TimeLineEventType.PLAN_BUILD: return "FPBP";
                case TimeLineEventType.BUILD_COMPLETED: return "FPBC";
                case TimeLineEventType.GATE_RELEASED: return "FPGR";
                case TimeLineEventType.WHOLE_SALE: return "FPWS";
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