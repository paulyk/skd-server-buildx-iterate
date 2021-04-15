using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class PlantService_Test : TestBase {
        
        public PlantService_Test() {
            context = GetAppDbContext();
        }

        [Fact]
        public async Task can_add_plant() {
            // setup
            var input = new PlantInput {
                Code = Gen_PlantCode(),
                Name = Get_Code(EntityFieldLen.Plant_Name)
            };

            var service = new PlantService(context);

            // test
            var before_count = await context.Plants.CountAsync();
            var payload = await service.CreatePlant(input);


            var after_count = await context.Plants.CountAsync();

            Assert.Equal(before_count + 1, after_count);

            var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.Code);
            Assert.Equal(input.Code, plant.Code);
            Assert.Equal(input.Name, plant.Name);
        }


    }
}