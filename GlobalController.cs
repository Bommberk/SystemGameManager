namespace Krassheiten.SystemGameManager.Functions;

using System;
using System.Collections;
using System.Reflection;

class GlobalFunctions
{
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