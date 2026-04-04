namespace Krassheiten.SystemGameManager.Functions;

using System;
using System.Collections;
using System.Reflection;

class GlobalFunctions
{
    private static int clogCount = 0;
    public static void ConsoleLog(string message)
    {
        Console.WriteLine(message);
    }

    public static void ConsoleError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void die(string? message = null)
    {
        if (message != null)
        {
            ConsoleError(message);
        }
        Environment.Exit(0);
    }

    /// <summary>
    /// Gibt eine Trennlinie in der Konsole aus, um Debug-Logs oder wichtige Informationen hervorzuheben.
    /// Nützlich, um die Übersicht in der Konsole zu verbessern und wichtige Abschnitte zu markieren.
    /// "S" steht hierbei  für short, da es nur eine einfache Trennlinie ohne zusätzliche Informationen ist.
    /// </summary>
    public static void slog()
    {
        Console.WriteLine("======== Debug Log ============");
    }

    /// <summary>
    /// Zählt die Anzahl der Aufrufe dieser Funktion und gibt sie in der Konsole aus.
    /// Nützlich für Debugging-Zwecke, um zu sehen, wie oft eine Funktion aufgerufen wird.
    /// "c" steht hierbei für count, da es die Anzahl der Aufrufe zählt und anzeigt.
    /// </summary>
    public static void clog()
    {
        clogCount++;
        Console.WriteLine($"======== Debug Log: {clogCount} ============");
    }

    /// <summary>
    /// Gibt eine benutzerdefinierte Nachricht zusammen mit einem Debug-Log aus.
    /// Nützlich, um spezifische Informationen oder Kontext in den Debug-Logs zu haben.
    /// "m" steht hierbei für message, da es eine benutzerdefinierte Nachricht in den Debug-Log integriert.
    /// </summary>
    public static void mlog(string message)
    {
        Console.WriteLine($"======== Debug Log: {message} ============");
    }

    public static void dump(object? obj, int indent = 0)
    {
        string indentStr = new string(' ', indent);
        if (obj == null)
        {
            Console.WriteLine($"{indentStr}null");
            return;
        }

        Type type = obj.GetType();
        // 🔹 Primitive / einfache Typen
        if (type.IsPrimitive || obj is string || obj is decimal)
        {
            Console.WriteLine($"{indentStr}{obj}");
            return;
        }
        // 🔹 IEnumerable (Arrays, Listen, etc.)
        if (obj is IEnumerable enumerable)
        {
            Console.WriteLine($"{indentStr}[");

            foreach (var item in enumerable)
            {
                dump(item, indent + 2);
            }

            Console.WriteLine($"{indentStr}]");
            return;
        }
        // 🔹 Objekte / Records
        Console.WriteLine($"{indentStr}{type.Name} {{");
        var properties = type.GetProperties();
        foreach (var prop in properties)
        {
            object? value = prop.GetValue(obj);
            Console.Write($"{indentStr}  {prop.Name}: ");

            if (value == null)
            {
                Console.WriteLine("null");
            }
            else if (prop.PropertyType.IsPrimitive || value is string)
            {
                Console.WriteLine(value);
            }
            else
            {
                Console.WriteLine();
                dump(value, indent + 4);
            }
        }
        Console.WriteLine($"{indentStr}}}");
    }
}