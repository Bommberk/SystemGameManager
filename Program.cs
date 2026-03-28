using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Infos;

internal static class Program
{
    private static void Main(string[] args)
    {
        GetInfoAsync();
    }

    private static void GetInfoAsync()
    {
        var pcInfo = new PcInfo();
        var gameInfo2 = new GameInfo2();
        var gameInfo = new GameInfo();
        Console.WriteLine("==============================");
        Console.WriteLine("         INFORMATIONEN        ");
        Console.WriteLine("==============================");
        // pcInfo.Write();
        gameInfo.Write();
        // gameInfo2.Write();
    }

}
