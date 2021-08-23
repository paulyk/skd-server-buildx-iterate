using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Dcws;
using HotChocolate;
using System.Threading.Tasks;
using SKD.Service;
using SKD.Model;
using HotChocolate.Types;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.SignalR;

namespace SKD.Server {
    [ExtendObjectType(typeof(Mutation))]
    public class DevMutation {

        private readonly bool IsDevelopment;

        public DevMutation() { }
        public DevMutation(bool isDevelopment) {
            IsDevelopment = isDevelopment;
        }

        public async Task<MutationPayload<string>> RemoveShipment(
                    [Service] SkdContext context,
                    string plantCode,
                    int sequence
                ) {

            if (!IsDevelopment) {
                throw new Exception("Dev mode only");
            }

            var payload = new MutationPayload<string>();

            var shipment = await context.Shipments
                .Include(t => t.ShipmentLots)
                    .ThenInclude(t => t.Invoices)
                    .ThenInclude(t => t.HandlingUnits)
                    .ThenInclude(t => t.Parts)
                .Include(t => t.ShipmentLots)
                    .ThenInclude(t => t.Invoices)
                    .ThenInclude(t => t.HandlingUnits)
                    .ThenInclude(t => t.Received)
                .Where(x => x.Plant.Code == plantCode)
                .Where(x => x.Sequence == sequence)
                .FirstOrDefaultAsync();

            if (shipment == null) {
                payload.Payload = "Shipment Not found";
                return payload;
            }


            shipment.ShipmentLots.ToList().ForEach(sl => {
                context.ShipmentLots.Remove(sl);
                sl.Lot = null;

                sl.Invoices.ToList().ForEach(inv => {
                    context.ShipmentInvoices.Remove(inv);
                    inv.HandlingUnits.ToList().ForEach(hu => {
                        hu.Received.ToList().ForEach(huReceived => {
                            context.HandlingUnitReceived.Remove(huReceived);
                        });
                        context.HandlingUnits.Remove(hu);

                    });
                });
            });

            context.Shipments.Remove(shipment);

            await context.SaveChangesAsync();

            payload.Payload = $"removed shipment {plantCode}-{shipment.Sequence}";
            return payload;
        }

        public async Task<MutationPayload<string>> RemoveBom(
            [Service] SkdContext context,
            string plantCode,
            int sequence

        ) {
            if (!IsDevelopment) {
                throw new Exception("Dev mode only");
            }

            var payload = new MutationPayload<string>();

            var bom = await context.Boms
                .Include(x => x.Lots).ThenInclude(t => t.LotParts)
                .Where(x => x.Plant.Code == plantCode && x.Sequence == sequence)
                .FirstOrDefaultAsync();

            if (bom == null) {
                payload.Payload = $"BOM {plantCode}-{sequence} not found";
                return payload;
            }

            bom.Lots.ToList().ForEach(lot => {
                lot.LotParts.ToList().ForEach(lotPart => {
                    context.LotParts.Remove(lotPart);
                });
                context.Lots.Remove(lot);
            });
            context.Boms.Remove(bom);

            await context.SaveChangesAsync();

            payload.Payload = $"removed BOM {plantCode}-{bom.Sequence}";
            return payload;
        }
    }
}