#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SKD.Common;
using SKD.Service;

namespace SKD.Service {
    public class BomFileParser {

        private static readonly FlatFileLine<BomFileLayout.Header> headerLineParser = new();
        private static readonly FlatFileLine<BomFileLayout.Detail> detailLineParser = new();

        public BomFileParser() {
        }
        public MutationPayload<BomLotPartDTO> BuildBomLotPartInput(string text) {
            MutationPayload<BomLotPartDTO> payload = new();

            try {
                var (headerTextLine, detailTextLines) = ParseTextLines(text);

                payload.Payload = new BomLotPartDTO {
                    PlantCode = headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_KD_PLANT_GSDB),
                    Sequence = Int16.Parse(headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_BRIDGE_SEQ_NBR)),
                    BomFileCreatedAt = headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_DATE_CREATED),
                    LotParts = BuildLotParts(detailTextLines)
                };
                return payload;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                payload.Errors.Add(new Error("", "Error parsing bom text"));
                return payload;
            }
        }

        private List<BomLotPartDTO.BomLotPartItem> BuildLotParts(List<string> detailTextLines) {
            var lotParts = new List<BomLotPartDTO.BomLotPartItem>();

            foreach (var lineText in detailTextLines) {
                var builder = new FlatFileLine<BomFileLayout.Detail>();
                var partType = detailLineParser.GetFieldValue(lineText, t => t.KBM_KIT_PART_TYPE);
                if (partType == "KIT") {
                    var kitSeqNo = detailLineParser.GetFieldValue(lineText, t => t.KBM_KIT_NUMBER);
                    var lotNo = detailLineParser.GetFieldValue(lineText, t => t.KBM_LOT_NUMBER);
                    var partNo = detailLineParser.GetFieldValue(lineText, t => t.KBM_NO_PART);
                    var partDesc = detailLineParser.GetFieldValue(lineText, t => t.KBM_PART_DESCRIPTION);
                    var quantity = (int)Double.Parse(detailLineParser.GetFieldValue(lineText, t => t.KBM_NET_PART_QTY));

                    var kitNo = $"{lotNo}{kitSeqNo}";
                    var modelCode = kitNo.Substring(0, 7);

                    var lotPart = lotParts.FirstOrDefault(t => t.LotNo == lotNo && t.PartNo == partNo);
                    if (lotPart == null) {
                        lotPart = new BomLotPartDTO.BomLotPartItem {
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
        public MutationPayload<BomLotKitDTO> BuildBomLotKitInput(string text) {
            MutationPayload<BomLotKitDTO> payload = new();

            try {
                var (headerTextLine, detailTextLines) = ParseTextLines(text);

                payload.Payload = new BomLotKitDTO {
                    PlantCode = headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_KD_PLANT_GSDB),
                    BomFileCreatedAt = headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_DATE_CREATED),
                    Sequence = Int16.Parse(headerLineParser.GetFieldValue(headerTextLine, t => t.HDR_BRIDGE_SEQ_NBR)),
                    Lots = BuildKitInputLots(detailTextLines)
                };
                return payload;
            } catch (Exception) {
                payload.Errors.Add(new Error("", "Error parsing bom text"));
                return payload;
            }
        }
        private List<BomLotKitDTO.LotEntry> BuildKitInputLots(List<string> detailTextLines) {
            var lots = new List<BomLotKitDTO.LotEntry>();

            foreach (var lineText in detailTextLines) {
                var partType = detailLineParser.GetFieldValue(lineText, t => t.KBM_KIT_PART_TYPE);
                if (partType == "KIT") {
                    var lotNo = detailLineParser.GetFieldValue(lineText, t => t.KBM_LOT_NUMBER);
                    var kitSeqNo = detailLineParser.GetFieldValue(lineText, t => t.KBM_KIT_NUMBER);

                    var kitNo = $"{lotNo}{kitSeqNo}";
                    var modelCode = kitNo.Substring(0, 7);

                    var lot = lots.FirstOrDefault(t => t.LotNo == lotNo);
                    if (lot == null) {
                        lot = new BomLotKitDTO.LotEntry {
                            LotNo = lotNo,
                            Kits = new List<BomLotKitDTO.LotEntry.LotKit>()
                        };
                        lots.Add(lot);

                    }
                    var lotKit = lot.Kits.FirstOrDefault(t => t.KitNo == kitNo);
                    if (lotKit == null) {
                        lot.Kits.Add(new BomLotKitDTO.LotEntry.LotKit {
                            KitNo = kitNo,
                            ModelCode = modelCode
                        });
                    }
                }
            }
            return lots;
        }

        private static (string headerLineText, List<string> detailTextLines) ParseTextLines(string text) {
            var lines = text.Split('\n')
                // remove emply lines
                .Where(t => t.Length > 0).ToList();

            var headerTextLine = "";
            var detailTextLines = new List<string>();
            if (lines.Count > 0) {
                headerTextLine = lines[0];
            }
            if (lines.Count > 2) {
                detailTextLines = lines
                    // skip header
                    .Skip(1)
                    // exclude trailer
                    .Take(lines.Count - 2).ToList();
            }
            return (headerTextLine, detailTextLines);
        }
    }
}