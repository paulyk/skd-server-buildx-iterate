namespace SKD.Server;

public static class AppSettingsKey {
    public static readonly string ExecutionTimeoutSeconds = "ExecutionTimeoutSeconds";
    public static readonly string DcwsServiceAddress = "DcwsServiceAddress";
    public static readonly string DefaultConnectionString = "Default";
    public static readonly string AllowGraphqlIntrospection = "AllowGraphqlIntrospection";
}

public class AppSettings {

    private readonly IConfiguration Configuration;
    public AppSettings(IConfiguration config) {
        this.Configuration = config;
    }

    public int ExecutionTimeoutSeconds {
        get {
            Int32.TryParse(Configuration[AppSettingsKey.ExecutionTimeoutSeconds], out int executionTimeoutSeconds);
            return executionTimeoutSeconds;
        }
    }

    public bool AllowGraphqlIntrospection {
        get {
            bool.TryParse(Configuration[AppSettingsKey.AllowGraphqlIntrospection], out bool allowGraphqlIntrospection);
            return allowGraphqlIntrospection;
        }
    }

    public string DefaultConnectionString {
        get {
            return Configuration.GetConnectionString(AppSettingsKey.DefaultConnectionString);
        }
    }

    public string DcwsServiceAddress {
        get {
            return Configuration[AppSettingsKey.DcwsServiceAddress];
        }
    }
}