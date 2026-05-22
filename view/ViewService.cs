namespace SystemGameManager.Service;

class ViewService
{
    public static string GetVersionFromReleases()
    {
        try
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                if (version.Revision > 0)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        catch { }
        
        return "Unknown";
    }
}