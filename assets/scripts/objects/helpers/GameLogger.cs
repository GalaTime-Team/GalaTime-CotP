using System;
using System.Collections.Generic;
using Godot;

/// <summary> Logs messages to the console and has helper methods to make it look better and useful. </summary>
public class GameLogger
{
    /// <summary> Console colors. Godot converts them to ASCI colors and prints them. Since Godot API doesn't have any enum or array of colors is used. So, this enum is used. </summary>
    public enum ConsoleColor { Black, Red, Green, Lime, Yellow, Blue, Magenta, Pink, Purple, Cyan, White, Orange, Gray }
    /// <summary> Level of log. Log type has different colors and names. </summary>
    public enum LogType { Info, Warning, Error, Success }
    /// <summary> Different log types and their colors and names to display in the console. </summary>
    public Dictionary<LogType, (string name, ConsoleColor color)> LogTypes = new()
    {
        { LogType.Info, ("INFO", ConsoleColor.Cyan) },
        { LogType.Warning, ("WARNING", ConsoleColor.Yellow) },
        { LogType.Error, ("ERROR", ConsoleColor.Red) },
        { LogType.Success, ("SUCCESS", ConsoleColor.Lime) }
    };

    /// <summary> Name, that will be displayed in the console. </summary>
    public string Name;
    public ConsoleColor Color;

    public GameLogger(string name, ConsoleColor color) => (Name, Color) = (name, color);

    public static string FormatConsoleColor(ConsoleColor color) => Enum.GetName(typeof(ConsoleColor), color).ToLower();

    /// <summary> Log a message to the console. Log type is Info by default. </summary>
    public void Log(string message, LogType type = LogType.Info) 
    {
        var colorType = FormatConsoleColor(LogTypes[type].color);
        var nameType = LogTypes[type].name;
        GD.PrintRich($"[[color={colorType}]{nameType}[/color]] [color={FormatConsoleColor(Color)}]{Name}[/color][color=gray]:[/color] {message}");
    }

    /// <summary> Log if condition is true. Otherwise, do nothing. </summary>
    public void LogIf(bool condition, string message, LogType type = LogType.Info) { if (condition) Log(message, type); }
}