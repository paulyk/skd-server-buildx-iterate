using SKD.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace SKD.Seed {
    internal class SeedDataGenerator {
        private SkdContext ctx;

        public SeedDataGenerator(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task Seed_VehicleTimelineVentType() {

            // in order by when they should occur
            var eventTypes = new List<KitTimelineEventType> {
                new KitTimelineEventType {
                    Code = TimeLineEventType.CUSTOM_RECEIVED.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.PLAN_BUILD.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.BULD_COMPLETED.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.GATE_RELEASED.ToString(),
                },
                new KitTimelineEventType {
                    Code = TimeLineEventType.WHOLE_SALE.ToString(),
                },
            };

            var sequence = 1;
            eventTypes.ForEach(eventType => {
                eventType.Description = UnderscoreToPascalCase(eventType.Code);
                eventType.Sequecne = sequence++;
            });

            ctx.KitTimelineEventTypes.AddRange(eventTypes);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.ProductionStations.Count()} vehicle timeline event types");
        }

        public async Task Seed_ProductionStations(ICollection<ProductionStation_Mock_DTO> data) {
            var stations = data.ToList().Select(x => new ProductionStation() {
                Code = x.code,
                Name = x.name,
                Sequence = x.sortOrder,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.ProductionStations.AddRange(stations);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.ProductionStations.Count()} production stations");
        }
        public async Task Seed_Components(ICollection<Component_MockData_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.code,
                Name = x.name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Added {ctx.Components.Count()} components");
        }

        private string UnderscoreToPascalCase(string input) {
            var str = input.Split("_").Aggregate((x, y) => x + "  " + y);
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }

    }
}