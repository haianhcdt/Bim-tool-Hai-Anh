using System;
using System.IO;

namespace DSCons.Revit.Starter.Infrastructure;

public static class Log
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DSCons", "RevitApiKit", "revit-api-kit.log");

    public static void Write(string message)
    {
        var directory = Path.GetDirectoryName(LogPath);
        if (directory is null) return;
        Directory.CreateDirectory(directory);
        File.AppendAllText(LogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
    }
}
