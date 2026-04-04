namespace Krassheiten.SystemGameManager.Service;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

class LauncherVdfService
{
    public static string? GetLibraryFolderPathFromVdf(Dictionary<string, object> vdfData)
    {
        foreach (var kvp in vdfData)
        {
            if(kvp.Key == "0")
                continue;
            if (kvp.Key.StartsWith("path", StringComparison.OrdinalIgnoreCase) && kvp.Value is string path)
            {
                return path;
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                string? result = GetLibraryFolderPathFromVdf(nestedDict);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
        }
        return null;
    }
    public static Dictionary<string, object>? LoadVdfAsArray(string filePath)
    {
        if(!File.Exists(filePath))
            return null;

        string content = File.ReadAllText(filePath);

        var vdf = VdfConvert.Deserialize(content);
        var root = vdf.Value as VObject;

        if(root == null)
            return null;
        return ConvertVObjectToDictionary(root);
    }
    private static Dictionary<string, object> ConvertVObjectToDictionary(VObject obj)
    {
        var dict = new Dictionary<string, object>();

        foreach (var item in obj)
        {
            if (item.Value is VValue value)
            {
                dict[item.Key] = value.Value ?? string.Empty;
            }
            else if (item.Value is VObject childObj)
            {
                dict[item.Key] = ConvertVObjectToDictionary(childObj);
            }
        }
        return dict;
    }
}