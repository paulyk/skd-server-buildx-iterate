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