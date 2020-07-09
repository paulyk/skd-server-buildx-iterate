using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentServiceTest : TestBase {

        private SkdContext ctx;
        public ComponentServiceTest() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

                [Fact]
        private async Task can_save_new_component(){
            var service = new ComponentService(ctx);
            var component = new Component() {
                Code = Util.RandomString(EntityMaxLen.Component_Code),
                Name = Util.RandomString(EntityMaxLen.Component_Name)
            };

            var priorComponentCount = await ctx.Components.CountAsync();
            var payload = await service.SaveComponent(component);

            Assert.NotNull(payload.Entity);
            var newCount = await ctx.Components.CountAsync();
            Assert.Equal(priorComponentCount + 1, newCount);            
        }

        [Fact]
        private async Task can_update_component() {
            var service = new ComponentService(ctx);
            var component = await ctx.Components.FirstOrDefaultAsync();
            
            var newCode = Util.RandomString(EntityMaxLen.Component_Code);
            var newName = Util.RandomString(EntityMaxLen.Component_Name);
            var oldCreatedAt = component.CreatedAt;

            component.Code = newCode;
            component.Name = newName;


            var priorComponentCount = await ctx.Components.CountAsync();
            var payload = await service.SaveComponent(component);
            var newCount = await ctx.Components.CountAsync();

            Assert.Equal(priorComponentCount, newCount);

            var modifiedComponent = await ctx.Components.FirstOrDefaultAsync(t => t.Id == component.Id);
            Assert.Equal(newCode, component.Code);
            Assert.Equal(newName, component.Name);
            Assert.Equal(oldCreatedAt, component.CreatedAt);
        }

        [Fact]
        private async Task validate_component_warns_duplicate_code() {
            var service = new ComponentService(ctx);

            var existingComponent = await ctx.Components.FirstAsync();

            var component = new Component() {
                Code = existingComponent.Code,
                Name = new String('x',EntityMaxLen.Component_Code)
            };

            var errors = await service.ValidateCreateComponent<Component>(component);

            errors.ForEach(error => {
                Console.WriteLine($"{error.Path},  {error.Message}");
            });

            var errorCount = errors.Count;
            Assert.Equal(1, errorCount);

            if (errors.Count > 0) {
                Assert.Equal("duplicate code", errors.First().Message);
            }
        }


        [Fact]
        private async Task validate_component_warns_duplicate_name() {
            var service = new ComponentService(ctx);

            var existingComponent = await ctx.Components.FirstAsync();

            var component = new Component() {
                Code = new String('x', EntityMaxLen.Component_Code), 
                Name = existingComponent.Name
            };

            var errors = await service.ValidateCreateComponent<Component>(component);

            errors.ForEach(error => {
                Console.WriteLine($"{error.Path},  {error.Message}");
            });

            var errorCount = errors.Count;
            Assert.Equal(1, errorCount);

            if (errors.Count > 0) {
                Assert.Equal("duplicate name", errors.First().Message);
            }
        }

        private void GenerateSeedData() {
            var components = new List<Component>() {
                new Component() { Code = "COMP1", Name = "Component name 1" },
                new Component() { Code = "COMP2", Name = "Component name 2" }
            };

            ctx.Components.AddRange(components);
            ctx.SaveChanges();
        }
    }
}