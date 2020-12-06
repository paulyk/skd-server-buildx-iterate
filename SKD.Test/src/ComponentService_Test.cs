using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentServiceTest : TestBase {

        public ComponentServiceTest() {
            ctx = GetAppDbContext();
            // GenerateSeedData();
        }

        [Fact]
        private async Task can_save_new_component() {
            var service = new ComponentService(ctx);
            var componentDTO = new ComponentInput() {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name)
            };

            var before_count = await ctx.Components.CountAsync();
            var payload = await service.SaveComponent(componentDTO);

            Assert.NotNull(payload.Entity);
            var expectedCount = before_count + 1;
            var actualCount = ctx.Components.Count();
            Assert.Equal(expectedCount, actualCount);
            Console.WriteLine(payload.Entity.Id);            
        }

        [Fact]
        private async Task can_update_component() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

            var component = await ctx.Components.FirstOrDefaultAsync();

            var before_CreatedAt = component.CreatedAt;
            var before_ComponentCount = await ctx.Components.CountAsync();

            var newCode = Gen_ComponentCode();
            var newName = Gen_ComponentCode() + "name";
            // test
            var service = new ComponentService(ctx);
            var payload = await service.SaveComponent(new ComponentInput {
                Id = component.Id,
                Code = newCode,
                Name = newName
            });

            // assert
            var after_ComponentCount = await ctx.Components.CountAsync();

            Assert.Equal(before_ComponentCount, after_ComponentCount);
            Assert.True(before_CreatedAt == payload.Entity.CreatedAt, "CreatedAt should not change when on saving existing component");

            var modifiedComponent = await ctx.Components.FirstOrDefaultAsync(t => t.Id == component.Id);
            Assert.Equal(newCode, component.Code);
            Assert.Equal(newName, component.Name);
            Assert.Equal(before_CreatedAt, component.CreatedAt);
        }

        [Fact]
        private async Task validate_component_warns_duplicate_code() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

            var existingComponent = await ctx.Components.FirstAsync();

            var component = new Component() {
                Code = existingComponent.Code,
                Name = new String('x', EntityFieldLen.Component_Code)
            };

            // test
            var service = new ComponentService(ctx);
            var errors = await service.ValidateCreateComponent<Component>(component);

            // assert
            var errorCount = errors.Count();
            Assert.Equal(1, errorCount);

            if (errors.Count > 0) {
                Assert.Equal("duplicate code", errors.First().Message);
            }
        }

        [Fact]
        private async Task can_save_multiple_component() {
            var before_count = ctx.Components.Count();
            var componentService = new ComponentService(ctx);

            // first
            await componentService.SaveComponent(new ComponentInput {
                Code = "AA", Name = "AA Name"
            });
            await componentService.SaveComponent(new ComponentInput {
                Code = "BB", Name = "BB Name"
            });

            var atterCount = ctx.Components.Count();

            Assert.Equal(before_count + 2, atterCount);
        }

        [Fact]
        private async Task can_modify_componetn_code() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());
            var component = await ctx.Components.FirstOrDefaultAsync();

            var newCode = Util.RandomString(EntityFieldLen.Component_Code).ToString();
            // test
            var service = new ComponentService(ctx);
            var payload = await service.SaveComponent(new ComponentInput {
                Id = component.Id,
                Code = newCode,
                Name = component.Name
            });

            var errorCount = payload.Errors.Count();
            Assert.Equal(0, errorCount);
            Assert.Equal(newCode, payload.Entity.Code);
        }

        [Fact]
        private async Task can_remove_componet() {
            var service = new ComponentService(ctx);
            var before_count = ctx.Components.Count();

            var dto = new ComponentInput {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name),
            };

            var payload = await service.SaveComponent(dto);

            var after_count = ctx.Components.Count();
            Assert.Equal(before_count + 1, after_count);
            Assert.Null(payload.Entity.RemovedAt);

            var payload2 = await service.RemoveComponent(payload.Entity.Id);
            Assert.NotNull( payload.Entity.RemovedAt);
        }

         [Fact]
        private async Task can_restore_componet() {
            var service = new ComponentService(ctx);

            // setup

            var dto = new ComponentInput {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name),
            };

            var payload = await service.SaveComponent(dto);
            Assert.Null(payload.Entity.RemovedAt);

            var payload2 = await service.RemoveComponent(payload.Entity.Id);
            Assert.NotNull( payload.Entity.RemovedAt);

            // test
            var payload3 = service.RestoreComponent(payload2.Entity.Id);
            Assert.Null( payload.Entity.RemovedAt);
        }
       
        [Fact]
        private async Task validate_component_warns_duplicate_name() {
            var before_count = ctx.Components.Count();
            var componentService = new ComponentService(ctx);

            // first
            await componentService.SaveComponent(new ComponentInput {
                Code = "AA", Name = "AA Name"
            });
            var count = ctx.Components.Count();
            Assert.Equal(before_count + 1, count);

            // try add with duplicate name
            var payload = await componentService.SaveComponent(new ComponentInput {
                Code = "BB", Name = "AA Name"
            });

            var errorCount = payload.Errors.Count();
            Assert.Equal(1, errorCount);
            Assert.Equal("duplicate name", payload.Errors.First().Message);
        }
    }
}