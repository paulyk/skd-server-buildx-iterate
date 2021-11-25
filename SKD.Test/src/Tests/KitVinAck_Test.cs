namespace SKD.Test;
public class KitVinAck_Test : TestBase {

    public KitVinAck_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();
    }

    [Fact]
    public async Task Can_Generate_KitVIn_Ack() {
        // setup
        var plant = await context.Plants.FirstAsync();
        var sequence = 2;
        var kitVinImport = await CreateKitVinImport(plant.Code, sequence);

        // test
        var service = new KitVinAckBuilder(context);
        var result = await service.GenerateKitVinAcknowledgment(kitVinImport.Plant.Code, kitVinImport.Sequence);

        // plant code
        Assert.Equal(plant.Code, result.PlantCode);

        // filename
        var expectedFilenameStart = $"{KitVinAckLayout.HDR_FILE_NAME_VAL}_{plant.Code}_{sequence.ToString().PadLeft(6, '0')}";
        var actualFilename = result.Filename.Substring(0, expectedFilenameStart.Length);
        Assert.Equal(expectedFilenameStart, actualFilename);

        // lines
        var (headerLine, detailLine, trailerLine) = ParsePayload(result.PayloadText);

        // header
        var headerBuilder = new FlatFileLine<KitVinAckLayout.Header>();
        // header plant code
        var expectedHeaerPlant = kitVinImport.Plant.Code;
        var actualHeaderPlant = headerBuilder.GetFieldValue(headerLine, t => t.HDR_KD_PLANT_GSDB);
        Assert.Equal(expectedHeaerPlant, actualHeaderPlant);

        // header sequence
        var expectedHeaderSequence = kitVinImport.Sequence.ToString().PadLeft(6, '0');
        var actualHeaderSequence = headerBuilder.GetFieldValue(headerLine, t => t.HDR_SEQ_NBR);
        Assert.Equal(expectedHeaderSequence, actualHeaderSequence);

        var exptectedHeaderFileName = KitVinAckLayout.HDR_FILE_NAME_VAL;
        var actualHeaderFilename = headerBuilder.GetFieldValue(headerLine, t => t.HDR_FILE_NAME).Trim();
        Assert.Equal(exptectedHeaderFileName, actualHeaderFilename);

        // detail
        var detailLineBuilder = new FlatFileLine<KitVinAckLayout.Detail>();

        // KVM_ACK_RECORD_TYPE
        var expected_KVM_ACK_RECORD_TYPE = KitVinAckLayout.DTL_KVM_ACK_RECORD_TYPE_VAL;
        var actual_KVM_ACK_RECORD_TYPE = detailLineBuilder.GetFieldValue(detailLine, t => t.KVM_ACK_RECORD_TYPE);
        Assert.Equal(expected_KVM_ACK_RECORD_TYPE, actual_KVM_ACK_RECORD_TYPE);

        // KVM_ACK_FILE_STATUS
        var expected_KVM_ACK_FILE_STATUS = "ACCEPTED";
        var actual_KVM_ACK_FILE_STATUS = detailLineBuilder.GetFieldValue(detailLine, t => t.KVM_ACK_FILE_STATUS).Trim();
        Assert.Equal(expected_KVM_ACK_FILE_STATUS, actual_KVM_ACK_FILE_STATUS);

        // KVM_ACK_TOTAL_DTL_RECORD record count
        var expected_KVM_ACK_TOTAL_DTL_RECORD = kitVinImport.KitVins.Count();
        var actual_KVM_ACK_TOTAL_DTL_RECORD = int.Parse(detailLineBuilder.GetFieldValue(detailLine, t => t.KVM_ACK_TOTAL_DTL_RECORD).Trim());
        Assert.Equal(expected_KVM_ACK_TOTAL_DTL_RECORD, actual_KVM_ACK_TOTAL_DTL_RECORD);

        // KVM_ACK_TOTAL_DTL_RECORD acceptec count
        var expected_KVM_ACK_TOTAL_DTL_ACCEPTED = kitVinImport.KitVins.Count();
        var actual_KVM_ACK_TOTAL_DTL_ACCEPTED = int.Parse(detailLineBuilder.GetFieldValue(detailLine, t => t.KVM_ACK_TOTAL_DTL_ACCEPTED).Trim());
        Assert.Equal(expected_KVM_ACK_TOTAL_DTL_ACCEPTED, actual_KVM_ACK_TOTAL_DTL_ACCEPTED);

        // KVM_ACK_TOTAL_DTL_REJECTED rejected count
        var expected_KVM_ACK_TOTAL_DTL_REJECTED = 0;
        var actual_KVM_ACK_TOTAL_DTL_REJECTED = int.Parse(detailLineBuilder.GetFieldValue(detailLine, t => t.KVM_ACK_TOTAL_DTL_REJECTED).Trim());
        Assert.Equal(expected_KVM_ACK_TOTAL_DTL_REJECTED, actual_KVM_ACK_TOTAL_DTL_REJECTED);

        (string headerLine, string detailLine, string trailerLine) ParsePayload(string text) {
            var x = text.Split('\n');
            return (headerLine: x[0], detailLine: x[1], trailerLine: x[2]);
        }
    }

    private async Task<KitVinImport> CreateKitVinImport(string plantCode, int sequence) {

        var lot = context.Lots.First();
        var plant = context.Plants.FirstOrDefault(t => t.Code == plantCode);
        var partnerPlantCode = Gen_PartnerPLantCode();
        var planBuildLeadTimeDays = 6;

        var input = new VinFile {
            PlantCode = plant.Code,
            PartnerPlantCode = partnerPlantCode,
            Sequence = sequence,
            Kits = lot.Kits.Select(t => new VinFile.VinFileKit {
                LotNo = lot.LotNo,
                KitNo = t.KitNo,
                VIN = Gen_VIN()
            }).ToList()
        };

        var service = new KitService(context, DateTime.Now, planBuildLeadTimeDays);
        var result = await service.ImportVIN(input);
        return result.Payload;
    }
}
