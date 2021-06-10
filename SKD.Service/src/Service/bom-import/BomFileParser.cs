#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SKD.Service.Util;

namespace SKD.Service {

    public class BomFileParser {

        /// <summary>File text lines</summary>
        private List<string> detailLines = new List<string>();

        public string HeaderLine = "";
        public List<string> DetailLines = new List<string>();
        public BomFileParser(string text) {
            var lines = text.Split('\n').ToList();
            if (lines.Count > 0) {
                HeaderLine = lines[0];
            }
            if (lines.Count > 2) {
                DetailLines = lines.Skip(1).Take(lines.Count - 2).ToList();
            }
        }

        public List<FlatFileLine.FieldValue> ParseHeaderLine() {
            var headerBuilder = new FlatFileLine(new BomFileLayout.Header());
            return headerBuilder.Parse(HeaderLine);
        }

        public List<List<FlatFileLine.FieldValue>> ParseDetailLines() {
            var detailLineBuilder = new FlatFileLine(new BomFileLayout.Detail());

            var detailLines = new List<List<FlatFileLine.FieldValue>>();
            foreach (var text in DetailLines) {
                var values = detailLineBuilder.Parse(text);
                detailLines.Add(values);
            }

            return detailLines;
        }

        public BomLotPartInput BuildBomLotPartInput() {

            var headeValues = ParseHeaderLine();
            var input = new BomLotPartInput {
                PlantCode = headeValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_KD_PLANT_GSDB)).Value,
                Sequence = Int16.Parse(headeValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_BRIDGE_SEQ_NBR)).Value),
                LotParts = BuildLotParts()
            };
            return input;
        }

        private List<BomLotPartInput.LotPart> BuildLotParts() {
            var detailLines = ParseDetailLines();
            var lotParts = new List<BomLotPartInput.LotPart>();
            foreach (var lineValues in detailLines) {
                var partType = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_KIT_PART_TYPE)).Value;
                if (partType == "KIT") {
                    var lotNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_LOT_NUMBER)).Value;
                    var kitSeqNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_KIT_NUMBER)).Value;
                    var partNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_NO_PART)).Value;
                    var partDesc = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_PART_DESCRIPTION)).Value;
                    var quantity = Int32.Parse(lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_NET_PART_QTY)).Value);

                    var kitNo = $"{lotNo}{kitSeqNo}";
                    var modelCode = kitNo.Substring(0, 7);

                    var lotPart= lotParts.FirstOrDefault(t => t.LotNo == lotNo && t.PartNo == partNo);
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
            var headeValues = ParseHeaderLine();
            var input = new BomLotKitInput {
                PlantCode = headeValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_KD_PLANT_GSDB)).Value,
                Sequence = Int16.Parse(headeValues.First(t => t.Name == nameof(BomFileLayout.Header.HDR_BRIDGE_SEQ_NBR)).Value),
                Lots = BuildKitInputLots()
            };
            return input;
        }

        private List<BomLotKitInput.Lot> BuildKitInputLots() {
            var detailLines = ParseDetailLines();
            var lots = new List<BomLotKitInput.Lot>();
            foreach (var lineValues in detailLines) {
                var partType = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_KIT_PART_TYPE)).Value;
                if (partType == "KIT") {
                    var lotNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_LOT_NUMBER)).Value;
                    var kitSeqNo = lineValues.First(t => t.Name == nameof(BomFileLayout.Detail.KBM_KIT_NUMBER)).Value;

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