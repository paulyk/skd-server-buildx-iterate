
namespace SKD.Service;

public class PartnerStatusAckService {


    public PartnerStatusAckDTO Parse(string text) {

        var result = new PartnerStatusAckDTO();

        var (headerLine, detailLine) = ParseLines(text);

        FlatFileLine<PartnerStatusAckLayout.HeaderLine> headerLineParser = new();
        FlatFileLine<PartnerStatusAckLayout.DetailLine> detailLineParser = new();

        result.PlantCode = headerLineParser.GetFieldValue(headerLine, t => t.HDR_KD_PLANT_GSDB);
        result.PartnerPlantCode = headerLineParser.GetFieldValue(headerLine, t => t.HDR_PARTNER_GSDB);
        result.Sequence = Int32.Parse(headerLineParser.GetFieldValue(headerLine, t => t.HDR_SEQ_NBR));

        result.FileDate = headerLineParser.GetFieldValue(headerLine, t => t.HDR_BATCH_DATE);

        var status = detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_FILE_STATUS);

        result.Accepted = status.Trim() == "ACCEPTED";
        result.TotalProcessed = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_RECORD));

        result.TotalAccepted = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_ACCEPTED));
        result.TotalRejected = Int32.Parse(detailLineParser.GetFieldValue(detailLine, t => t.PST_ACK_TOTAL_DTL_REJECTED));

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