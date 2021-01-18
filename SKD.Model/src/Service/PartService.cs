using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class PartService {

        private readonly SkdContext context;

        private List<Part> PartsList = new List<Part>();
        public PartService(SkdContext ctx) {
            this.context = ctx;
        }

        public static string ReFormatPartNo(string part) {
            return Regex.Replace(part, @"(^[- ]+|[ ]|[- ]*$)", "");
        }

        public async Task<List<Part>> GetEnsureParts(List<(string partNo, string partDesc)> inputParts) {
            var parts = new List<Part>();

            foreach (var inputPart in inputParts) {
                var formattedPartNo = ReFormatPartNo(inputPart.partNo);
                if (!parts.Any(t => t.PartNo == formattedPartNo)) {
                    var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == formattedPartNo);
                    if (part == null) {
                        part = new Part {
                            PartNo = formattedPartNo,
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
}