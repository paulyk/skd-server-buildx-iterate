using System;
using System.Collections.Generic;
using SKD.VCS.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.VCS.Test {
    public class ProductionPlantService_Test : TestBase {

        private SkdContext ctx;
        public ProductionPlantService_Test() {
            ctx = GetAppDbContext();
            
        }        

        [Fact]
        public async Task can_create_production_plant () {
            // setup
            var dto = new ProductionPlantDTO {
                Code = Util.RandomString(EntityFieldLen.ProductionPlant_Code).ToUpper(),
                Name = Util.RandomString(EntityFieldLen.ProductionPlant_Name).ToUpper(),
            };

            var before_count = ctx.ProductionPlants.Count();
            // act
            var service = new ProductionPlantService(ctx);
            var payload = await service.CreateProductionPlant(dto);

            // assert
            var after_count = ctx.ProductionPlants.Count();
            Assert.Equal(1, after_count);
        }
    }
}