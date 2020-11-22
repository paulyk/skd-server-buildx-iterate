using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ProductionStationServiceTest : TestBase {

        private SkdContext ctx;
        public ProductionStationServiceTest() {
            ctx = GetAppDbContext();
        }

        [Fact]
        private async Task can_save_new_production_station() {
            var service = new ProductionStationService(ctx);
            var productionStationDTO = new ProductionStationInput() {
                Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
                Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
            };

            var before_count = await ctx.Components.CountAsync();
            var payload = await service.SaveProductionStation(productionStationDTO);

            Assert.NotNull(payload.Entity);
            var expectedCount = before_count + 1;
            var actualCount = ctx.ProductionStations.Count();
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        private async Task can_update_new_production_station() {
            var service = new ProductionStationService(ctx);
            var productionStationDTO = new ProductionStationInput() {
                Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
                Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
            };

            var before_count = await ctx.Components.CountAsync();
            var payload = await service.SaveProductionStation(productionStationDTO);

            var expectedCount = before_count + 1;
            var firstCount = ctx.ProductionStations.Count();
            Assert.Equal(expectedCount, firstCount);

            // update
            var newCode = Util.RandomString(EntityFieldLen.ProductionStation_Code);
            var newName = Util.RandomString(EntityFieldLen.ProductionStation_Name);

            var updatedPayload = await service.SaveProductionStation(new ProductionStationInput {
                Id = payload.Entity.Id,
                Code = newCode,
                Name = newName
            });

            var secondCount = ctx.ProductionStations.Count();
            Assert.Equal(firstCount, secondCount);
            Assert.Equal(newCode, updatedPayload.Entity.Code);
            Assert.Equal(newName, updatedPayload.Entity.Name);
        }


        [Fact]
        private async Task cannot_add_duplicate_production_station() {
            // setup
            var service = new ProductionStationService(ctx);

            var code = Util.RandomString(EntityFieldLen.ProductionStation_Code).ToString();
            var name = Util.RandomString(EntityFieldLen.ProductionStation_Name).ToString();

            var count_1 = ctx.ProductionStations.Count();
            var payload = await service.SaveProductionStation(new ProductionStationInput {
                Code = code,
                Name = name
            });

            var count_2 = ctx.ProductionStations.Count();
            Assert.Equal(count_1 + 1, count_2);

            // insert again
            var payload2 = await service.SaveProductionStation(new ProductionStationInput {
                Code = code,
                Name = name
            });


            var count_3 = ctx.ProductionStations.Count();
            Assert.Equal(count_2, count_3);

            var errorCount = payload2.Errors.Count();
            Assert.Equal(2, errorCount);

            var duplicateCode = payload2.Errors.Any(e => e.Message == "duplicate code");
            Assert.True(duplicateCode, "expected: 'duplicateion code`");

            var duplicateName = payload2.Errors.Any(e => e.Message == "duplicate name");
            Assert.True(duplicateCode, "expected: 'duplicateion name`");
        }
    }
}
