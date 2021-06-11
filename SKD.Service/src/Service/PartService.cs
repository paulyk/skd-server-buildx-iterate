using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Model;

namespace SKD.Common {

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
}