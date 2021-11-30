
namespace SKD.Service;

public class PartService {

    private readonly SkdContext context;

    public PartService(SkdContext ctx) {
        this.context = ctx;
    }

    public static string ReFormatPartNo(string part) {
        // replace leading and trailng '-', ' '
        return Regex.Replace(part, @"(^[- ]+|[ ]|[- ]*$)", "");
    }

    public async Task<List<Part>> GetEnsureParts(List<(string partNo, string partDesc)> inputParts) {
        var parts = new List<Part>();

        foreach (var inputPart in inputParts) {
            if (!parts.Any(t => t.PartNo == inputPart.partNo)) {
                var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == inputPart.partNo);
                if (part == null) {
                    part = new Part {
                        PartNo = inputPart.partNo,
                        OriginalPartNo = inputPart.partNo,
                        PartDesc = inputPart.partDesc
                    };
                    context.Parts.Add(part);
                }
                parts.Add(part);
            }
        }

        return parts;
    }
}
