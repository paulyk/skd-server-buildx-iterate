#nullable enable
namespace SKD.Service;

public static class ApplicationSetting {

    public static async Task<AppSetting> GetAppSetting(SkdContext context, AppSettingCode appSettingCode)
        => await context.AppSettings.Where(t => t.Code == appSettingCode.ToString()).FirstAsync();

    public static async Task<string> GetAppSettingValue(SkdContext context, AppSettingCode appSettingCode)
        => await context.AppSettings
            .Where(t => t.Code == appSettingCode.ToString())
            .Select(t => t.Value)
            .FirstAsync();

    public static async Task<int> GetAppSettingValueInt(SkdContext context, AppSettingCode appSettingCode) {
        var value = await GetAppSettingValue(context, appSettingCode);
        return !String.IsNullOrWhiteSpace(value) ? int.Parse(value) : 0;
    }

    public static async Task<AppSettingsDTO> GetKnownAppSettings(SkdContext context) {
        var result = await context.AppSettings.ToListAsync();
        return new AppSettingsDTO {
            PlanBuildLeadTimeDays = GetValueInt(result, AppSettingCode.PlanBuildLeadTimeDays),
            WholeSaleCutoffDays = GetValueInt(result, AppSettingCode.WholeSaleCutoffDays),
            VerifyVinLeadTimeDays = GetValueInt(result, AppSettingCode.VerifyVinLeadTimeDays),
            EngineComponentCode = result
                .Where(t => t.Code == AppSettingCode.EngineComponentCode.ToString())
                .Select(t => t.Value)
                .First()
        };

        static int GetValueInt(IEnumerable<AppSetting> appSettings, AppSettingCode appSettingCode) {
            var result = appSettings
                .Where(t => t.Code == appSettingCode.ToString())
                .Select(t => t.Value)
                .FirstOrDefault();

            return result != null ? int.Parse(result) : 0;
        }
    }
}

public class AppSettingsDTO {
    public int PlanBuildLeadTimeDays;
    public int WholeSaleCutoffDays;
    public int VerifyVinLeadTimeDays;
    public string EngineComponentCode = "";

}