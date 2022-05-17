
namespace SKD.Service;

public class PartnerStatusAckParser {
    private static readonly FlatFileLine<PartnerStatusAckLayout.HeaderLine> headerLineParser = new();
    private static readonly FlatFileLine<PartnerStatusAckLayout.DetailLine> detailLineParser = new();

    public PartnerStatusAckFile Parse(string text) {

        var result = new PartnerStatusAckFile();

        var (headerLine, detailLine) = ParseLines(text);

        result.PlantCode = headerLineParser.GetFieldValue(headerLine, t => t.HDR_KD_PLANT_GSDB);
        result.PartnerPlantCode = headerLineParser.GetFieldValue(headerLine, t => t.HDR_PARTNER_GSDB);
        result.Sequence = Int32.Parse(headerLineParser.GetFieldValue(headerLine, t => t.HDR_SEQ_NBR));
        result.FileDate = headerLineParser.GetFieldValue(headerLine, t => t.HDR_BATCH_DATE);

        // DTLACCEPTED  000000031900000003190000000000
        result.Status = detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_FILE_STATUS);
        result.Total = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_RECORD));
        var tmp1 = detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_ACCEPTED);
        result.Accepted = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_ACCEPTED));
        result.Rejected = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_REJECTED));

        return result;
    }

    private static (string headerLine, string detailLine) ParseLines(string text) {
        var lines = text.Split('\n').Where(t => t.Length > 0).ToList();

        var headerLine = "";
        var detailLine = "";
        if (lines.Count > 0) {
            headerLine = lines[0];
        }
        if (lines.Count > 1) {
            detailLine = lines[1];
        }
        return (headerLine, detailLine);
    }

}