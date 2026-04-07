namespace Krassheiten.SystemGameManager.Service;

class DatabaseService
: DatabaseController
{
    public void SaveNewRecord(object record)
    {
        
    }

    public void UpdateRecord(object record)
    {
        
    }
    public void showAllDatabases()
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
        using var reader = command.ExecuteReader();
        Console.WriteLine("Tables in the database:");
        while (reader.Read())        {
            Console.WriteLine($"- {reader.GetString(0)}");
        }
    }

    public void showTable(object tableName)
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = "SELECT * FROM Games;";
        using var reader = command.ExecuteReader();
        Console.WriteLine("Games in the database:");
        while (reader.Read())
        {
            Console.WriteLine($"- Id: {reader.GetInt32(0)}, Name: {reader.GetString(1)}, InstallFolderPath: {reader.GetString(2)}, ExePath: {reader.GetString(3)}, GameVolumePercent: {reader.GetInt32(4)}, MusicVolumePercent: {reader.GetInt32(5)}");
        }
    }
}