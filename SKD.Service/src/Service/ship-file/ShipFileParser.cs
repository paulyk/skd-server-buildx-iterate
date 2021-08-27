#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SKD.Common;
using SKD.Model;
using SKD.Service;

namespace SKD.Service {

    public class ShipFileParser {

        public ShipFile ParseShipmentFile(string text) {

            var shipmentInput = new ShipFile();

            var (headerLine, detailLines) = ParseLines(text);

            var (plantCode, sequence, dateCreated) = ParseHeaderLine(headerLine);
            shipmentInput.PlantCode = plantCode;
            shipmentInput.Sequence = sequence;
            shipmentInput.Created = dateCreated;

            ShipFileLot? currentLot = null;
            ShipFileInvoice? currentInvoice = null;

            foreach (var line in detailLines) {
                var lineType = GetLineType(line);
                if (lineType == LineType.Lot) {
                    currentLot = ParseLotLine(line);
                    shipmentInput.Lots.Add(currentLot);
                } else if (lineType == LineType.Invoice) {
                    currentInvoice = ParseInvoiceLine(line);
                    if (currentLot != null) {
                        currentLot.Invoices.Add(currentInvoice);
                    }
                } else if (lineType == LineType.Part) {
                    var part = ParsePartLine(line);
                    if (currentInvoice != null) {
                        currentInvoice.Parts.Add(part);
                    }
                }
            }

            return shipmentInput;
        }

        public (string plantCode, int sequence, DateTime dateCreated ) ParseHeaderLine(string line)  {
            var lineParser = new FlatFileLine<ShipFileLayout.HeaderLine>();
            var plantCode = lineParser.GetFieldValue(line, t => t.HDR_CD_PLANT);
            var sequence = Int32.Parse(lineParser.GetFieldValue(line, t => t.HDR_BRIG_SEQ_NO));

            // date created
            var dateStr = lineParser.GetFieldValue(line, t => t.HDR_DATE_CREATED);
            CultureInfo provider = CultureInfo.InvariantCulture;
            var format = "yyyyMMdd";
            var dateCreated = DateTime.ParseExact(dateStr, format, provider);

            return (plantCode, sequence, dateCreated);
        }

        public ShipFileLot ParseLotLine(string line) {
            var lineParser = new FlatFileLine<ShipFileLayout.LotLine>();
            return new ShipFileLot {
                LotNo = lineParser.GetFieldValue(line, t => t.LOT_NUMBER),
                Invoices = new List<ShipFileInvoice>()
            };
        }

        public ShipFileInvoice ParseInvoiceLine(string line) {
            var lineParser = new FlatFileLine<ShipFileLayout.InvoiceLine>();
            return new ShipFileInvoice {
                InvoiceNo = lineParser.GetFieldValue(line, t => t.NO_INVOICE),
                ShipDate = DateTime.Parse(lineParser.GetFieldValue(line, t => t.DT_SHIPPED)),
                Parts = new List<ShipFilePart>()
            };
        }

        public ShipFilePart ParsePartLine(string line) {
            var lineParser = new FlatFileLine<ShipFileLayout.PartLine>();
            return new ShipFilePart {
                PartNo = lineParser.GetFieldValue(line, t => t.NO_PART),
                HandlingUnitCode = lineParser.GetFieldValue(line, t => t.HANDLER_UNIT_CODE),
                CustomerPartNo = lineParser.GetFieldValue(line, t => t.NO_PART_BUS),
                CustomerPartDesc = lineParser.GetFieldValue(line, t => t.DS_PART),
                Quantity = Int32.Parse(lineParser.GetFieldValue(line, t => t.QT_SHIPPED))
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

        private LineType GetLineType(string line) {
            var lineParser = new FlatFileLine<ShipFileLayout.LotLine>();
            var lineType = lineParser.GetFieldValue(line, t => t.CD_TYPE);

            switch (lineType) {
                case "01": return LineType.Lot;
                case "02": return LineType.Invoice;
                case "03": return LineType.Part;
                case "RH": return LineType.HeaderTrailer;
                default: throw new Exception("Unknown line type");
            }
        }

        enum LineType {
            Lot,
            Invoice,
            Part,
            HeaderTrailer
        };

    }
}

