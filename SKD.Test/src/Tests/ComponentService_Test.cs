namespace SKD.Test;
public class ComponentServiceTest : TestBase {

    public ComponentServiceTest() {
        context = GetAppDbContext();
        // GenerateSeedData();
    }

    [Fact]
    public async Task Can_save_new_component() {
        var service = new ComponentService(context);
        var input = new ComponentInput() {
            Code = Util.RandomString(EntityFieldLen.Component_Code),
            Name = Util.RandomString(EntityFieldLen.Component_Name)
        };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveComponent(input);

        Assert.NotNull(result.Payload);
        var expectedCount = before_count + 1;
        var actualCount = await context.Components.CountAsync();
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public async Task Can_set_component_serial_requirement() {
        var service = new ComponentService(context);
        var input = new ComponentInput() {
            Code = Util.RandomString(EntityFieldLen.Component_Code),
            Name = Util.RandomString(EntityFieldLen.Component_Name),
            DcwsSerialCaptureRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS
        };

        var before_count = await context.Components.CountAsync();
        var result = await service.SaveComponent(input);

        var component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.Code);
        Assert.Equal(input.DcwsSerialCaptureRule, component.ComponentSerialRule);

        // modify
        input.Id = result.Payload.Id;
        input.DcwsSerialCaptureRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS;
        await service.SaveComponent(input);
        component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.Code);
        Assert.Equal(input.DcwsSerialCaptureRule, component.ComponentSerialRule);
    }

    [Fact]
    public async Task Can_update_component() {
        // setup
        Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());

        var component = await context.Components.FirstOrDefaultAsync();

        var before_CreatedAt = component.CreatedAt;
        var before_ComponentCount = await context.Components.CountAsync();

        var newCode = Gen_ComponentCode();
        var newName = Gen_ComponentCode() + "name";
        // test
        var service = new ComponentService(context);
        var result = await service.SaveComponent(new ComponentInput {
            Id = component.Id,
            Code = newCode,
            Name = newName
        });

        // assert
        var after_ComponentCount = await context.Components.CountAsync();

        Assert.Equal(before_ComponentCount, after_ComponentCount);
        Assert.True(before_CreatedAt == result.Payload.CreatedAt, "CreatedAt should not change when on saving existing component");

        var modifiedComponent = await context.Components.FirstOrDefaultAsync(t => t.Id == component.Id);
        Assert.Equal(newCode, component.Code);
        Assert.Equal(newName, component.Name);
        Assert.Equal(before_CreatedAt, component.CreatedAt);
    }


    [Fact]
    public async Task Can_save_multiple_component() {
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
    public async Task Can_modify_componetn_code() {
        // setup
        Gen_Components(Gen_ComponentCode(), Gen_ComponentCode());
        var component = await context.Components.FirstOrDefaultAsync();

        var newCode = Util.RandomString(EntityFieldLen.Component_Code).ToString();
        // test
        var service = new ComponentService(context);
        var result = await service.SaveComponent(new ComponentInput {
            Id = component.Id,
            Code = newCode,
            Name = component.Name
        });

        var errorCount = result.Errors.Count;
        Assert.Equal(0, errorCount);
        Assert.Equal(newCode, result.Payload.Code);
    }

    [Fact]
    public async Task Can_remove_componet() {
        var service = new ComponentService(context);
        var before_count = context.Components.Count();

        var dto = new ComponentInput {
            Code = Util.RandomString(EntityFieldLen.Component_Code),
            Name = Util.RandomString(EntityFieldLen.Component_Name),
        };

        var result = await service.SaveComponent(dto);

        var after_count = context.Components.Count();
        Assert.Equal(before_count + 1, after_count);
        Assert.Null(result.Payload.RemovedAt);

        await service.RemoveComponent(result.Payload.Id);
        Assert.NotNull(result.Payload.RemovedAt);
    }

    [Fact]
    public async Task Can_restore_componet() {
        var service = new ComponentService(context);

        // setup

        var dto = new ComponentInput {
            Code = Util.RandomString(EntityFieldLen.Component_Code),
            Name = Util.RandomString(EntityFieldLen.Component_Name),
        };

        var result = await service.SaveComponent(dto);
        Assert.Null(result.Payload.RemovedAt);

        var result_2 = await service.RemoveComponent(result.Payload.Id);
        Assert.NotNull(result.Payload.RemovedAt);

        // test
        await service.RestoreComponent(result_2.Payload.Id);
        Assert.Null(result.Payload.RemovedAt);
    }

    [Fact]
    public async Task Validate_component_warns_duplicate_code() {
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
        var errorCount = errors.Count;
        Assert.Equal(1, errorCount);

        Assert.Equal("duplicate code", errors.First().Message);
    }

    [Fact]
    public async Task Validate_component_warns_duplicate_name() {
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
        var errorCount = errors.Count;
        Assert.Equal(1, errorCount);

        Assert.Equal("duplicate name", errors.First().Message);
    }

}
