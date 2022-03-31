#nullable enable
namespace SKD.Model;

public class AppSetting : EntityBase {
    public string Code { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
}

public enum AppSettingCode {
    PlanBuildLeadTimeDays = 0,
    WholeSaleCutoffDays,
    VerifyVinLeadTimeDays,
    EngineComponentCode,
}

