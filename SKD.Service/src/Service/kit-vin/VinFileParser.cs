#nullable enable

namespace SKD.Service;

public class VinFileParser {

    private static readonly FlatFileLine<VinFileLayout.HeaderLine> headerLineParser = new();
    private static readonly FlatFileLine<VinFileLayout.DetailLine> detailLineParser = new();

    public VinFile ParseVinFile(string text) {
        var kitVinFile = new VinFile();

        var (headerLine, detailLines) = ParseLines(text);

        var (plantCode, partnerPlantCode, sequence) = ParseHeaderLine(headerLine);
        kitVinFile.PlantCode = plantCode;
        kitVinFile.PartnerPlantCode = partnerPlantCode;
        kitVinFile.Sequence = sequence;

        foreach (var line in detailLines) {
            kitVinFile.Kits.Add(ParseDetailLine(line));
        }

        return kitVinFile;
    }


    public (string plantCode, string partnerPlantCode, int sequence) ParseHeaderLine(string line) {
        var plantCode = headerLineParser.GetFieldValue(line, t => t.HDR_KD_PLANT_GSDB);
        var partnerPlantCode = headerLineParser.GetFieldValue(line, t => t.HDR_PARTNER_GSDB);
        var sequence = Int32.Parse(headerLineParser.GetFieldValue(line, t => t.HDR_SEQ_NBR));

        return (plantCode, partnerPlantCode, sequence);
    }

    public VinFile.VinFileKit ParseDetailLine(string line) {
        return new VinFile.VinFileKit {
            LotNo = detailLineParser.GetFieldValue(line, x => x.KVM_LOT_NUMBER),
            KitNo = detailLineParser.GetFieldValue(line, x => x.KVM_KIT_NUMBER),
            VIN = detailLineParser.GetFieldValue(line, x => x.KVM_PHYSICAL_VIN)
        };
    }

    private static (string headerLine, List<string> detailLines) ParseLines(string text) {
        var lines = text.Split('\n')
            // remove emply lines
            .Where(t => t.Length > 0).ToList();

        var headerLine = "";
        var detailLines = new List<string>();
        if (lines.Count > 0) {
            headerLine = lines[0];
        }
        if (lines.Count > 2) {
            detailLines = lines
                // skip header
                .Skip(1)
                // exclude trailer
                .Take(lines.Count - 2).ToList();
        }
        return (headerLine, detailLines);
    }
}

