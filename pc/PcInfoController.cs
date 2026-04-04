namespace Krassheiten.SystemGameManager.Controller;

using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class PcInfoController
{
    private const long Gigabyte = 1024L * 1024 * 1024;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(4)
    };

    public string PcName { get; } = GetPcName();
    public string? LocalIpv4Address { get; } = GetLocalIpv4Address(GetActiveNetworkInterfaces());
    public string? LocalIpv6Address { get; } = GetLocalIpv6Address(GetActiveNetworkInterfaces());
    public string? PublicIpv4Address { get; } = GetPublicIpAddressAsync("https://api4.ipify.org").Result;
    public string? PublicIpv6Address { get; } = GetPublicIpAddressAsync("https://api6.ipify.org").Result;
    public string? MACAddress { get; } = GetMacAddress(GetActiveNetworkInterfaces());
    public string OSVersion { get; } = Environment.OSVersion.ToString();
    public string? CPU { get; } = GetCpuInfo();
    public string RAM { get; } = GetRamInfo();
    public string? GPU { get; } = GetGpuInfo();
    public string? Battery { get; } = GetBatteryStatus();
    public StorageInfo Storage { get; } = GetStorageInfo();

    public void Write()
    {
        Console.WriteLine("==============================");
        Console.WriteLine("        PC-Informationen      ");
        Console.WriteLine("==============================");
        WriteLine("PC-Name", PcName);
        WriteLine("IPv4 Lokal", LocalIpv4Address);
        WriteLine("IPv6 Lokal", LocalIpv6Address);
        WriteLine("IPv4 Public", PublicIpv4Address);
        WriteLine("IPv6 Public", PublicIpv6Address);
        WriteLine("MAC", MACAddress);
        WriteLine("OS", OSVersion);
        WriteLine("CPU", CPU);
        WriteLine("RAM", RAM);
        WriteLine("GPU", GPU);
        WriteLine("Akku", Battery);

        WriteLine("Storage", $"{Storage.TotalUsedGb}/{Storage.TotalSizeGb} GB (frei: {Storage.TotalFreeGb} GB)");

        foreach (var drive in Storage.Drives)
        {
            var driveName = string.IsNullOrWhiteSpace(drive.Label)
                ? drive.Letter
                : $"{drive.Letter} ({drive.Label})";

            var storageValue = $"{drive.UsedGb}/{drive.SizeGb} GB (frei: {drive.FreeGb} GB)";

            if (drive.VirtualHostDrive is not null)
            {
                storageValue += $" [virtuell auf {drive.VirtualHostDrive}]";
            }
            else if (drive.VirtualReservedGb > 0 && drive.VirtualDriveNames.Count > 0)
            {
                storageValue += $" (-{drive.VirtualReservedGb} GB fuer {string.Join(", ", drive.VirtualDriveNames)} virtuell)";
            }

            WriteLine($"Laufwerk {driveName}", storageValue);
        }
    }
    
    private static NetworkInterface[] GetActiveNetworkInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .ToArray();
    }

    private static string? GetLocalIpv4Address(IEnumerable<NetworkInterface> interfaces)
    {
        foreach (var ni in interfaces)
        {
            foreach (var addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    return addr.Address.ToString();
            }
        }
        return null;
    }

    private static string? GetLocalIpv6Address(IEnumerable<NetworkInterface> interfaces)
    {
        foreach (var ni in interfaces)
        {
            foreach (var addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetworkV6
                    && !addr.Address.IsIPv6LinkLocal)
                    return addr.Address.ToString();
            }
        }
        return null;
    }

    private static async Task<string?> GetPublicIpAddressAsync(string endpoint)
    {
        try
        {
            return (await Http.GetStringAsync(endpoint)).Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? GetMacAddress(IEnumerable<NetworkInterface> interfaces)
    {
        foreach (var ni in interfaces)
        {
            var bytes = ni.GetPhysicalAddress().GetAddressBytes();
            if (bytes.Length > 0)
                return string.Join("-", bytes.Select(b => b.ToString("X2")));
        }
        return null;
    }

    private static string? GetCpuInfo()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
        foreach (ManagementObject obj in searcher.Get())
            return obj["Name"]?.ToString();
        return null;
    }

    private static string? GetGpuInfo()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
        foreach (ManagementObject obj in searcher.Get())
            return obj["Name"]?.ToString();
        return null;
    }

    private static string GetRamInfo()
    {
        using var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
        long totalBytes = 0;
        foreach (ManagementObject obj in searcher.Get())
            totalBytes += Convert.ToInt64(obj["Capacity"]);
        return $"{totalBytes / Gigabyte} GB";
    }

    private static string GetPcName()
    {
        return Environment.MachineName;
    }

    private static StorageInfo GetStorageInfo()
    {
        // VHD-Info: virtueller Laufwerksbuchstabe -> (HostLaufwerk oder null, tatsaechliche Dateigroesse auf Host)
        Dictionary<string, (string? hostDrive, long fileSize)> vhdInfo = GetAttachedVhdInfo();

        // VHD-Dateigroessen pro Host-Laufwerk summieren
        // Fallback ohne Admin: unbekannten Host auf Systemlaufwerk mappen (typisch C:)
        var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\') ?? "C:";
        var vhdSizePerHost  = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var vhdNamesPerHost = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (vDrive, (host, fSize)) in vhdInfo)
        {
            var effectiveHost = host ?? systemDrive;
            if (!string.IsNullOrWhiteSpace(effectiveHost))
            {
                vhdSizePerHost.TryGetValue(effectiveHost, out var cur);
                vhdSizePerHost[effectiveHost] = cur + fSize;
                if (!vhdNamesPerHost.TryGetValue(effectiveHost, out var list))
                    vhdNamesPerHost[effectiveHost] = list = new();
                list.Add(vDrive);
            }
        }

        using var searcher = new ManagementObjectSearcher(
            "SELECT DeviceID, VolumeName, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
        var disks = searcher.Get().Cast<ManagementObject>().ToList();
        var diskByLetter = disks.ToDictionary(
            d => d["DeviceID"]?.ToString() ?? string.Empty,
            StringComparer.OrdinalIgnoreCase);

        // Wenn FileSize (ohne Admin) nicht verfuegbar ist, als Naeherung die GESAMTGROESSE
        // des virtuellen Laufwerks nutzen (nicht den belegten Platz).
        foreach (var (vDrive, (host, fSize)) in vhdInfo)
        {
            if (fSize > 0) continue;
            var effectiveHost = host ?? systemDrive;
            if (!diskByLetter.TryGetValue(vDrive, out var vDisk)) continue;

            var vSize = Convert.ToInt64(vDisk["Size"]);
            vhdSizePerHost.TryGetValue(effectiveHost, out var cur);
            vhdSizePerHost[effectiveHost] = Math.Max(cur, vSize);
        }

        // Gesamtgroesse/-nutzung bleibt roh (keine VHD-Subtraktion auf Gesamtwert)
        long totalSize = 0;
        long totalUsed = 0;
        long totalFree = 0;
        var diskInfos = new List<(string Letter, string? Label, long Size, long Free)>();
        foreach (var d in disks)
        {
            var letter = d["DeviceID"]?.ToString() ?? "?";
            var label = d["VolumeName"]?.ToString();
            var size = Convert.ToInt64(d["Size"]);
            var free = Convert.ToInt64(d["FreeSpace"]);

            totalSize += size;
            totalFree += free;
            totalUsed += size - free;
            diskInfos.Add((letter, label, size, free));
        }

        var drives = new List<DriveInfo>();

        foreach (var disk in diskInfos)
        {
            var letter = disk.Letter;
            var label = disk.Label;
            var size = disk.Size;
            var free = disk.Free;
            var used = size - free;

            if (vhdInfo.TryGetValue(letter, out var meta))
            {
                drives.Add(new DriveInfo(
                    letter,
                    label,
                    used / Gigabyte,
                    size / Gigabyte,
                    free / Gigabyte,
                    meta.hostDrive ?? systemDrive,
                    0,
                    Array.Empty<string>()));
            }
            else
            {
                vhdSizePerHost.TryGetValue(letter, out var vhdOnThis);
                var realUsed = Math.Max(0, used - vhdOnThis);
                var realSize = Math.Max(0, size - vhdOnThis);

                drives.Add(new DriveInfo(
                    letter,
                    label,
                    realUsed / Gigabyte,
                    realSize / Gigabyte,
                    free / Gigabyte,
                    null,
                    vhdOnThis / Gigabyte,
                    vhdOnThis > 0 && vhdNamesPerHost.TryGetValue(letter, out var vList)
                        ? vList.ToArray()
                        : Array.Empty<string>()));
            }
        }

        return new StorageInfo(
            totalUsed / Gigabyte,
            totalSize / Gigabyte,
            totalFree / Gigabyte,
            drives);
    }

    // Gibt zurueck: virtueller Laufwerksbuchstabe -> (HostLaufwerksbuchstabe oder null, Dateigroesse auf Host in Bytes)
    // Schritt 1 (kein Admin noetig): Win32_DiskDrive Caption prueft auf "virtual"
    // Schritt 2 (Admin noetig):       MSFT_DiskImage liefert Host-Pfad + Dateigroesse
    private static Dictionary<string, (string? hostDrive, long fileSize)> GetAttachedVhdInfo()
    {
        var result = new Dictionary<string, (string?, long)>(StringComparer.OrdinalIgnoreCase);

        // --- Schritt 1: virtuelle Laufwerke per Caption erkennen ---
        using var ddSearcher = new ManagementObjectSearcher("SELECT DeviceID, Caption FROM Win32_DiskDrive");
        foreach (ManagementObject dd in ddSearcher.Get())
        {
            var caption = dd["Caption"]?.ToString() ?? "";
            if (!caption.Contains("virt", StringComparison.OrdinalIgnoreCase)) continue;

            var devId     = dd["DeviceID"]?.ToString() ?? "";

            // DiskIndex aus DeviceID extrahieren (z.B. "\\.\PHYSICALDRIVE3" -> 3)
            var suffix = devId.LastIndexOf("PHYSICALDRIVE", StringComparison.OrdinalIgnoreCase);
            if (suffix < 0 || !uint.TryParse(devId[(suffix + 13)..], out var diskIndex)) continue;

            // Direkt über DiskIndex suchen (ASSOCIATORS von DiskDrive funktioniert nicht bei virtuellen)
            using var partSearcher = new ManagementObjectSearcher(
                $"SELECT DeviceID FROM Win32_DiskPartition WHERE DiskIndex={diskIndex}");

            foreach (ManagementObject part in partSearcher.Get())
            {
                var partId = part["DeviceID"]?.ToString() ?? "";
                using var logSearcher = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partId}'}} " +
                    "WHERE AssocClass=Win32_LogicalDiskToPartition");

                foreach (ManagementObject logDisk in logSearcher.Get())
                {
                    var dl = logDisk["DeviceID"]?.ToString();
                    if (!string.IsNullOrEmpty(dl))
                        result[dl] = (null, 0); // Host noch unbekannt
                }
            }
        }

        // --- Schritt 2: Host-Laufwerk + Dateigroesse via MSFT_DiskImage (braucht Admin) ---
        try
        {
            var scope = new ManagementScope(@"\\.\ROOT\Microsoft\Windows\Storage");
            scope.Connect();

            using var imgSearcher = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT ImagePath, FileSize, Number FROM MSFT_DiskImage WHERE Attached=TRUE"));

            foreach (ManagementObject img in imgSearcher.Get())
            {
                var imagePath = img["ImagePath"]?.ToString();
                if (string.IsNullOrEmpty(imagePath) || imagePath.Length < 2) continue;

                var fileSize   = Convert.ToInt64(img["FileSize"]);
                var diskNumber = Convert.ToUInt32(img["Number"]);
                var hostDrive  = imagePath[..2].ToUpper();

                using var partSearcher = new ManagementObjectSearcher(
                    $"SELECT DeviceID FROM Win32_DiskPartition WHERE DiskIndex={diskNumber}");

                foreach (ManagementObject part in partSearcher.Get())
                {
                    var partId = part["DeviceID"]?.ToString() ?? "";
                    using var logSearcher = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partId}'}} " +
                        "WHERE AssocClass=Win32_LogicalDiskToPartition");

                    foreach (ManagementObject logDisk in logSearcher.Get())
                    {
                        var dl = logDisk["DeviceID"]?.ToString();
                        if (!string.IsNullOrEmpty(dl))
                            result[dl] = (hostDrive, fileSize); // mit Host-Info anreichern
                    }
                }
            }
        }
        catch { /* kein Admin – nur Schritt 1 verfuegbar */ }

        return result;
    }

    private static string? GetBatteryStatus()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery");
        var results = searcher.Get().Cast<ManagementObject>().ToList();

        if (results.Count == 0)
            return null;

        var bat    = results[0];
        var charge = bat["EstimatedChargeRemaining"];
        var status = Convert.ToUInt16(bat["BatteryStatus"]);
        var statusText = status switch
        {
            1 => "Entladen",
            2 => "Lädt (AC)",
            3 => "Voll geladen",
            4 => "Niedrig",
            5 => "Kritisch",
            6 => "Lädt",
            7 => "Lädt, Hoch",
            8 => "Lädt, Niedrig",
            9 => "Lädt, Kritisch",
            _ => "Unbekannt"
        };

        return $"{charge}% - {statusText}";
    }

    private static void WriteLine(string label, string? value)
    {
        Console.WriteLine($"{label + ":",-14} {value ?? "nicht verfügbar"}");
    }

    public sealed record StorageInfo(long TotalUsedGb, long TotalSizeGb, long TotalFreeGb, IReadOnlyList<DriveInfo> Drives);

    public sealed record DriveInfo(
        string Letter,
        string? Label,
        long UsedGb,
        long SizeGb,
        long FreeGb,
        string? VirtualHostDrive,
        long VirtualReservedGb,
        IReadOnlyList<string> VirtualDriveNames);
}