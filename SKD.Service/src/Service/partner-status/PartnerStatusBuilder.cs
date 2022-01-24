#nullable enable

namespace SKD.Service;

public class PartnerStatusBuilder {

    private static readonly FlatFileLine<PartnerStatusLayout.Header> headerLineParser = new();
    private static readonly FlatFileLine<PartnerStatusLayout.Detail> detailLineParser = new();

    private readonly SkdContext context;
    public PartnerStatusBuilder(SkdContext context) {
        this.context = context;
    }

    public async Task<PartnerStatusDTO> GeneratePartnerStatusFilePaylaod(string plantCode, int sequence) {

        var kitSnapshotRun = await context.KitSnapshotRuns
            .Include(t => t.Plant)
            .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null).OrderBy(u => u.Kit.Lot.LotNo).ThenBy(u => u.Kit.KitNo)).ThenInclude(t => t.Kit)
                .ThenInclude(t => t.Lot)
            .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null).OrderBy(u => u.Kit.Lot.LotNo).ThenBy(u => u.Kit.KitNo)).ThenInclude(t => t.Kit)
                .ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .Where(t => t.Plant.Code == plantCode && t.Sequence == sequence)
            .FirstOrDefaultAsync();

        if (kitSnapshotRun == null) {
            return new PartnerStatusDTO {
                PlantCode = plantCode,
                Sequence = sequence,
                RunDate = (DateTime?)null,
                ErrorMessage = $"Kit snapshot not found for plant {plantCode} seq {sequence}",
                PayloadText = ""
            };
        }

        var lines = new List<string>();

        // header
        lines.Add(BuildHeaderLine(kitSnapshotRun));

        // detail            
        foreach (var snapshot in kitSnapshotRun.KitSnapshots.Where(t => t.RemovedAt == null)) {
            lines.Add(BuildDetailLine(snapshot));
        }

        // trailer
        lines.Add(BuildTrailerLine(kitSnapshotRun));

        var result = new PartnerStatusDTO {
            PlantCode = kitSnapshotRun.Plant.Code,
            Sequence = kitSnapshotRun.Sequence,
            RunDate = kitSnapshotRun.RunDate,
            Filename = await GenPartnerStatusFilename(kitSnapshotRun.Id),
            PayloadText = String.Join('\n', lines)
        };
        return result;
    }

    public async Task<string> GenPartnerStatusFilename(Guid kitSnapshotRunId) {
        var snapshotRun = await context.KitSnapshotRuns.Include(t => t.Plant)
            .FirstAsync(t => t.Id == kitSnapshotRunId);

        await Task.Delay(0);
        var formattedRunDate = snapshotRun.RunDate.ToString(PartnerStatusLayout.FILENAME_DATE_FORMAT);
        var prefix = PartnerStatusLayout.FILENAME_PREFIX;
        var plantCode = snapshotRun.Plant.Code;
        var partnerPlantCode = snapshotRun.Plant.PartnerPlantCode;
        return $"{prefix}_{plantCode}_{partnerPlantCode}_{formattedRunDate}.txt";
    }

    private string BuildHeaderLine(KitSnapshotRun snapshotRun) {
        var headerLayout = new PartnerStatusLayout.Header();

        var fields = new List<FlatFileLine<PartnerStatusLayout.Header>.FieldValue> {
                headerLineParser.CreateFieldValue(t => t.HDR_RECORD_TYPE, PartnerStatusLayout.Header.HDR_RECORD_TYPE_VAL),
                headerLineParser.CreateFieldValue(t => t.HDR_FILE_NAME, PartnerStatusLayout.Header.HDR_FILE_NAME_VAL),
                headerLineParser.CreateFieldValue(t => t.HDR_KD_PLANT_GSDB, snapshotRun.Plant.Code),
                headerLineParser.CreateFieldValue(t => t.HDR_PARTNER_GSDB,snapshotRun.Plant.PartnerPlantCode),
                headerLineParser.CreateFieldValue(t => t.HDR_PARTNER_TYPE, snapshotRun.Plant.PartnerPlantType),
                headerLineParser.CreateFieldValue(
                    t => t.HDR_SEQ_NBR,
                    snapshotRun.Sequence.ToString().PadLeft(headerLayout.HDR_SEQ_NBR,'0')),
                headerLineParser.CreateFieldValue(
                    t => t.HDR_BATCH_DATE,
                    snapshotRun.RunDate.ToString(PartnerStatusLayout.Header.HDR_BATCH_DATE_FORMAT)),
                headerLineParser.CreateFieldValue(
                    t => t.HDR_FILLER, new String(' ', headerLayout.HDR_FILLER)),
            };

        return headerLineParser.Build(fields);
    }

    private string BuildDetailLine(KitSnapshot snapshot) {
        var layout = new PartnerStatusLayout.Detail();

        var fields = new List<FlatFileLine<PartnerStatusLayout.Detail>.FieldValue> {
                detailLineParser.CreateFieldValue(t => t.PST_RECORD_TYPE, PartnerStatusLayout.PST_RECORD_TYPE_VAL),
                detailLineParser.CreateFieldValue(t => t.PST_TRAN_TYPE, snapshot.ChangeStatusCode.ToString()),
                detailLineParser.CreateFieldValue(t => t.PST_LOT_NUMBER, snapshot.Kit.Lot.LotNo),
                detailLineParser.CreateFieldValue(t => t.PST_KIT_NUMBER,snapshot.Kit.KitNo),
                detailLineParser.CreateFieldValue(t => t.PST_PHYSICAL_VIN, snapshot.VIN),

                detailLineParser.CreateFieldValue(
                    t => t.PST_BUILD_DATE,
                    snapshot.OrginalPlanBuild != null
                        ? snapshot.OrginalPlanBuild.Value.ToString(PartnerStatusLayout.PST_DATE_FORMAT)
                        : ""
                ),

                detailLineParser.CreateFieldValue(
                    t => t.PST_ACTUAL_DEALER_CODE,
                    snapshot.DealerCode ?? ""
                ),
                detailLineParser.CreateFieldValue(
                    t => t.PST_ENGINE_SERIAL_NUMBER,
                    snapshot.EngineSerialNumber
                ),
                detailLineParser.CreateFieldValue(
                    t => t.PST_CURRENT_STATUS,
                    ToFordTimelineCode(snapshot.KitTimeLineEventType.Code)
                ),

                detailLineParser.CreateFieldValue(t => t.PST_IP1R_STATUS_DATE, ""),
                detailLineParser.CreateFieldValue(t => t.PST_IP1S_STATUS_DATE, ""),
                detailLineParser.CreateFieldValue(t => t.PST_IP2R_STATUS_DATE, ""),
                detailLineParser.CreateFieldValue(t => t.PST_IP2S_STATUS_DATE,""),

                // custom receive
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPRE_STATUS_DATE,
                    FormattedDate(snapshot.CustomReceived, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                // plan build
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPBP_STATUS_DATE,
                    FormattedDate(snapshot.PlanBuild, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                // vin check
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPVC_STATUS_DATE,
                    FormattedDate(snapshot.VerifyVIN, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                // build complete
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPBC_STATUS_DATE,
                    FormattedDate(snapshot.BuildCompleted, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                // gate release
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPGR_STATUS_DATE,
                    FormattedDate(snapshot.GateRelease, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                // whole salse
                detailLineParser.CreateFieldValue(
                    t => t.PST_FPWS_STATUS_DATE,
                    FormattedDate(snapshot.Wholesale, PartnerStatusLayout.PST_STATUS_DATE_FORMAT)),
                detailLineParser.CreateFieldValue(t => t.PST_FILLER, "")
            };

        return detailLineParser.Build(fields);
    }

    public string BuildTrailerLine(KitSnapshotRun snapshotRun) {
        var layout = new PartnerStatusLayout.Trailer();
        var lineBuilder = new FlatFileLine<PartnerStatusLayout.Trailer>();

        var fields = new List<FlatFileLine<PartnerStatusLayout.Trailer>.FieldValue> {
                lineBuilder.CreateFieldValue(t => t.TLR_RECORD_TYPE, PartnerStatusLayout.TLR_RECORD_TYPE_VAL),
                lineBuilder.CreateFieldValue(t => t.TLR_FILE_NAME, PartnerStatusLayout.TLR_FILE_NAME_VAL),
                lineBuilder.CreateFieldValue(t => t.TLR_KD_PLANT_GSDB, snapshotRun.Plant.Code),
                lineBuilder.CreateFieldValue(t => t.TLR_PARTNER_GSDB, snapshotRun.Plant.PartnerPlantCode),
                lineBuilder.CreateFieldValue(
                    t => t.TLR_TOTAL_RECORDS,
                    // add 2 for Header + Trailer
                    (snapshotRun.KitSnapshots.Count + 2).ToString().PadLeft(layout.TLR_TOTAL_RECORDS, '0')),
                lineBuilder.CreateFieldValue(t => t.TLR_FILLER, ""),
            };

        return lineBuilder.Build(fields);
    }

    public string ToFordTimelineCode(TimeLineEventCode timeLineEventType) =>
        timeLineEventType switch {
            TimeLineEventCode.CUSTOM_RECEIVED => FordTimeLineCode.FPCR.ToString(),
            TimeLineEventCode.PLAN_BUILD => FordTimeLineCode.FPBP.ToString(),
            TimeLineEventCode.BUILD_COMPLETED => FordTimeLineCode.FPBC.ToString(),
            TimeLineEventCode.GATE_RELEASED => FordTimeLineCode.FPGR.ToString(),
            TimeLineEventCode.WHOLE_SALE => FordTimeLineCode.FPWS.ToString(),
            _ => throw new Exception("Unexpected timeline event")
        };


    private string FormattedDate(DateTime? date, string dateFormat) {
        return date.HasValue
            ? date.Value.ToString(dateFormat)
            : "";
    }
}
