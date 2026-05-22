namespace SystemGameManager.Games.Service;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

class LauncherVdfService
{
    public static string[] GetLibraryFolderPathFromVdf(Dictionary<string, object> vdfData)
    {
        var paths = new List<string>();
        CollectLibraryFolderPaths(vdfData, paths);

        return [.. paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    private static void CollectLibraryFolderPaths(Dictionary<string, object> vdfData, List<string> paths)
    {
        foreach (var kvp in vdfData)
        {
            if (kvp.Key.StartsWith("path", StringComparison.OrdinalIgnoreCase) && kvp.Value is string path)
            {
                paths.Add(path);
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                CollectLibraryFolderPaths(nestedDict, paths);
            }
        }
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