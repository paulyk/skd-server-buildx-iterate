#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SKD.Common;

namespace SKD.Common {

    public class BomFileParser {
        public string HeaderLineText = "";
        public List<string> DetailTextLines = new List<string>();
        public BomFileParser(string text) {
            var lines = text.Split('\n').ToList();
            if (lines.Count > 0) {
                HeaderLineText = lines[0];
            }
            if (lines.Count > 2) {
                DetailTextLines = lines.Skip(1).Take(lines.Count - 2).ToList();
            }
        }
        public BomLotPartInput BuildBomLotPartInput() {

            var headerLineBuilder = new FlatFileLine<BomFileLayout.Header>();

            var input = new BomLotPartInput {
                PlantCode = headerLineBuilder.GetFieldValue(HeaderLineText, t => t.HDR_KD_PLANT_GSDB),
                Sequence = Int16.Parse(headerLineBuilder.GetFieldValue(HeaderLineText, t => t.HDR_BRIDGE_SEQ_NBR)),
                LotParts = BuildLotParts()
            };
            return input;
        }
        private List<BomLotPartInput.LotPart> BuildLotParts() {
            var lotParts = new List<BomLotPartInput.LotPart>();
            var detailLineBuilder = new FlatFileLine<BomFileLayout.Detail>();

            foreach (var lineText in DetailTextLines) {


                var builder = new FlatFileLine<BomFileLayout.Detail>();
                var partType = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_KIT_PART_TYPE);
                if (partType == "KIT") {
                    var kitSeqNo = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_KIT_NUMBER);
                    var lotNo = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_LOT_NUMBER);
                    var partNo = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_NO_PART);
                    var partDesc = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_PART_DESCRIPTION);
                    var quantity = Int32.Parse(detailLineBuilder.GetFieldValue(lineText, t => t.KBM_NET_PART_QTY));

                    var kitNo = $"{lotNo}{kitSeqNo}";
                    var modelCode = kitNo.Substring(0, 7);

                    var lotPart = lotParts.FirstOrDefault(t => t.LotNo == lotNo && t.PartNo == partNo);
                    if (lotPart == null) {
                        lotPart = new BomLotPartInput.LotPart {
                            LotNo = lotNo,
                            PartNo = partNo,
                            PartDesc = partDesc,
                            Quantity = quantity
                        };
                        lotParts.Add(lotPart);
                    } else {
                        lotPart.Quantity += quantity;
                    }
                }
            }
            return lotParts;
        }
        public BomLotKitInput BuildBomLotKitInput() {
            var headerLineBuildr = new FlatFileLine<BomFileLayout.Header>();
            var input = new BomLotKitInput {
                PlantCode = headerLineBuildr.GetFieldValue(HeaderLineText, t => t.HDR_KD_PLANT_GSDB),
                Sequence = Int16.Parse(headerLineBuildr.GetFieldValue(HeaderLineText, t => t.HDR_BRIDGE_SEQ_NBR)),
                Lots = BuildKitInputLots()
            };
            return input;
        }
        private List<BomLotKitInput.Lot> BuildKitInputLots() {
            var lots = new List<BomLotKitInput.Lot>();
            var detailLineBuilder = new FlatFileLine<BomFileLayout.Detail>();

            foreach (var lineText in DetailTextLines) {
                var partType = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_KIT_PART_TYPE);
                if (partType == "KIT") {
                    var lotNo = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_LOT_NUMBER);
                    var kitSeqNo = detailLineBuilder.GetFieldValue(lineText, t => t.KBM_KIT_NUMBER);

                    var kitNo = $"{lotNo}{kitSeqNo}";
                    var modelCode = kitNo.Substring(0, 7);

                    var lot = lots.FirstOrDefault(t => t.LotNo == lotNo);
                    if (lot == null) {
                        lot = new BomLotKitInput.Lot {
                            LotNo = lotNo,
                            Kits = new List<BomLotKitInput.Lot.LotKit>()
                        };
                        lots.Add(lot);

                    }
                    var lotKit = lot.Kits.FirstOrDefault(t => t.KitNo == kitNo);
                    if (lotKit == null) {
                        lot.Kits.Add(new BomLotKitInput.Lot.LotKit {
                            KitNo = kitNo,
                            ModelCode = modelCode
                        });
                    }
                }
            }
            return lots;
        }
    }
}