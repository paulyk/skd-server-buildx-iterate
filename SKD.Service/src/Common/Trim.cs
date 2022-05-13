namespace SKD.Common;

public static class Trim {
    public static void TrimStringProperties<T>(T obj) {
        var properties = obj.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string)).ToList();

        foreach (var prop in properties) {
            var value = (string)prop.GetValue(obj, null);
            if (value != null) {
                prop.SetValue(obj, value.Trim());
            }
        }

    }
}
