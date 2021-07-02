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
    public class KitVinAckBuilder {

        private readonly SkdContext context;
        public KitVinAckBuilder(SkdContext context) {
            this.context = context;
        }

        public async Task<KitVinActDTO> GenerateKitVinAcknowledgment(string plantCode, int sequence) {
            KitVinImport kitVinImport = await context.KitVinImports.FirstOrDefaultAsync(t => t.Plant.Code == plantCode && t.Sequence == sequence);
            if (kitVinImport == null) {
                return new KitVinActDTO {
                    PlantCode = plantCode,
                    Sequence = sequence,
                    ErrorMessage = $"KitVinImport not found for {plantCode} - {sequence}",
                };
            }

            return new KitVinActDTO();
        }
    }
}