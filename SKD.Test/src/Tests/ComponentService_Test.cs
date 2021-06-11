using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Common;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentServiceTest : TestBase {

        public ComponentServiceTest() {
            context = GetAppDbContext();
            // GenerateSeedData();
        }

        [Fact]
        private async Task can_save_new_component() {
            var service = new ComponentService(context);
            var input = new ComponentInput() {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name)
            };

            var before_count = await context.Components.CountAsync();
            var payload = await service.SaveComponent(input);

            Assert.NotNull(payload.Payload);
            var expectedCount = before_count + 1;
            var actualCount = await context.Components.CountAsync();
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        private async Task can_set_component_serial_requirement() {
            var service = new ComponentService(context);
            var input = new ComponentInput() {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name),
                DcwsSerialCaptureRule = DcwsSerialCaptureRule.UNKNOWN
            };

            var before_count = await context.Components.CountAsync();
            var payload = await service.SaveComponent(input);

            var component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.Code);
            Assert.Equal(input.DcwsSerialCaptureRule, component.DcwsSerialCaptureRule);

            // modify
            input.Id = payload.Payload.Id;
            input.DcwsSerialCaptureRule = DcwsSerialCaptureRule.UNKNOWN;
            await service.SaveComponent(input);
            component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.Code);
            Assert.Equal(input.DcwsSerialCaptureRule, component.DcwsSerialCaptureRule);
        }

        [Fact]
        private async Task can_update_component() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

            var component = await context.Components.FirstOrDefaultAsync();

            var before_CreatedAt = component.CreatedAt;
            var before_ComponentCount = await context.Components.CountAsync();

            var newCode = Gen_ComponentCode();
            var newName = Gen_ComponentCode() + "name";
            // test
            var service = new ComponentService(context);
            var payload = await service.SaveComponent(new ComponentInput {
                Id = component.Id,
                Code = newCode,
                Name = newName
            });

            // assert
            var after_ComponentCount = await context.Components.CountAsync();

            Assert.Equal(before_ComponentCount, after_ComponentCount);
            Assert.True(before_CreatedAt == payload.Payload.CreatedAt, "CreatedAt should not change when on saving existing component");

            var modifiedComponent = await context.Components.FirstOrDefaultAsync(t => t.Id == component.Id);
            Assert.Equal(newCode, component.Code);
            Assert.Equal(newName, component.Name);
            Assert.Equal(before_CreatedAt, component.CreatedAt);
        }


        [Fact]
        private async Task can_save_multiple_component() {
            var before_count = context.Components.Count();
            var componentService = new ComponentService(context);

            // first
            await componentService.SaveComponent(new ComponentInput {
                Code = "AA", Name = "AA Name"
            });
            await componentService.SaveComponent(new ComponentInput {
                Code = "BB", Name = "BB Name"
            });

            var atterCount = context.Components.Count();

            Assert.Equal(before_count + 2, atterCount);
        }

        [Fact]
        private async Task can_modify_componetn_code() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());
            var component = await context.Components.FirstOrDefaultAsync();

            var newCode = Util.RandomString(EntityFieldLen.Component_Code).ToString();
            // test
            var service = new ComponentService(context);
            var payload = await service.SaveComponent(new ComponentInput {
                Id = component.Id,
                Code = newCode,
                Name = component.Name
            });

            var errorCount = payload.Errors.Count();
            Assert.Equal(0, errorCount);
            Assert.Equal(newCode, payload.Payload.Code);
        }

        [Fact]
        private async Task can_remove_componet() {
            var service = new ComponentService(context);
            var before_count = context.Components.Count();

            var dto = new ComponentInput {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name),
            };

            var payload = await service.SaveComponent(dto);

            var after_count = context.Components.Count();
            Assert.Equal(before_count + 1, after_count);
            Assert.Null(payload.Payload.RemovedAt);

            var payload2 = await service.RemoveComponent(payload.Payload.Id);
            Assert.NotNull(payload.Payload.RemovedAt);
        }

        [Fact]
        private async Task can_restore_componet() {
            var service = new ComponentService(context);

            // setup

            var dto = new ComponentInput {
                Code = Util.RandomString(EntityFieldLen.Component_Code),
                Name = Util.RandomString(EntityFieldLen.Component_Name),
            };

            var payload = await service.SaveComponent(dto);
            Assert.Null(payload.Payload.RemovedAt);

            var payload2 = await service.RemoveComponent(payload.Payload.Id);
            Assert.NotNull(payload.Payload.RemovedAt);

            // test
            var payload3 = service.RestoreComponent(payload2.Payload.Id);
            Assert.Null(payload.Payload.RemovedAt);
        }

        [Fact]
        private async Task validate_component_warns_duplicate_code() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

            var existingComponent = await context.Components.FirstAsync();

            var input = new ComponentInput() {
                Code = existingComponent.Code,
                Name = new String('x', EntityFieldLen.Component_Name)
            };

            // test
            var service = new ComponentService(context);
            var errors = await service.ValidateCreateComponent<ComponentInput>(input);

            // assert
            var errorCount = errors.Count();
            Assert.Equal(1, errorCount);

            Assert.Equal("duplicate code", errors.First().Message);
        }

        [Fact]
        private async Task validate_component_warns_duplicate_name() {
            // setup
            Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

            var existingComponent = await context.Components.FirstAsync();

            var input = new ComponentInput() {
                Code = new String('x', EntityFieldLen.Component_Code),
                Name = existingComponent.Name
            };

            // test
            var service = new ComponentService(context);
            var errors = await service.ValidateCreateComponent<ComponentInput>(input);

            // assert
            var errorCount = errors.Count();
            Assert.Equal(1, errorCount);

            Assert.Equal("duplicate name", errors.First().Message);
        }

    }
}