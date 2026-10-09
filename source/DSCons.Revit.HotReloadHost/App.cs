using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.HotReloadHost;

/// <summary>
/// The only assembly Revit loads from disk during development. Product code is
/// loaded from a fresh byte array whenever an existing Ribbon command is used.
/// </summary>
public sealed class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            try { application.CreateRibbonTab(HotReloadBranding.RibbonTabName); }
            catch { /* The branded tab is already present in this Revit session. */ }

            var panel = application.CreateRibbonPanel(HotReloadBranding.RibbonTabName, "Revit API Kit");
            var hostAssemblyPath = typeof(App).Assembly.Location;

            panel.AddItem(new PushButtonData(
                "DSConsWorkingView",
                "Working\n3D View",
                hostAssemblyPath,
                typeof(CreateWorkingViewProxyCommand).FullName)
            {
                ToolTip = "Tạo hoặc kích hoạt View 3D làm việc cá nhân (3D_Working_HaiAnh) trên Project Browser.",
                Image = SemanticRibbonIconFactory.Create("view3d", false),
                LargeImage = SemanticRibbonIconFactory.Create("view3d", true)
            });

            panel.AddItem(new PushButtonData(
                "DSConsBatchTag",
                "Batch\nTag",
                hostAssemblyPath,
                typeof(BatchTagProxyCommand).FullName)
            {
                ToolTip = "Ghi chú tag kích thước hàng loạt cho các đoạn ống gió trong View hiện hành.",
                Image = SemanticRibbonIconFactory.Create("tag", false),
                LargeImage = SemanticRibbonIconFactory.Create("tag", true)
            });

            panel.AddItem(new PushButtonData(
                "DSConsAlignTags",
                "Align\nTags",
                hostAssemblyPath,
                typeof(AlignTagsProxyCommand).FullName)
            {
                ToolTip = "Căn chỉnh các thẻ ghi chú (Tag) đã chọn thẳng hàng theo phương ngang hoặc phương dọc.",
                Image = SemanticRibbonIconFactory.Create("align", false),
                LargeImage = SemanticRibbonIconFactory.Create("align", true)
            });

            panel.AddItem(new PushButtonData(
                "DSConsAboutMe",
                "About\nMe",
                hostAssemblyPath,
                typeof(AboutMeProxyCommand).FullName)
            {
                ToolTip = "Xem thông tin branding của người sở hữu add-in.",
                Image = SemanticRibbonIconFactory.Create("about", false),
                LargeImage = SemanticRibbonIconFactory.Create("about", true)
            });

            HotReloadLog.Write("Hot Reload Host started.");
            return Result.Succeeded;
        }
        catch (Exception exception)
        {
            HotReloadLog.Write(exception.ToString());
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}

[Transaction(TransactionMode.Manual)]
public sealed class CreateWorkingViewProxyCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        => ReloadableCommandRunner.Execute("DSCons.Revit.Starter.Commands.CreateWorkingViewCommand", commandData, ref message, elements);
}

[Transaction(TransactionMode.Manual)]
public sealed class BatchTagProxyCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        => ReloadableCommandRunner.Execute("DSCons.Revit.Starter.Commands.BatchTagCommand", commandData, ref message, elements);
}

[Transaction(TransactionMode.Manual)]
public sealed class AlignTagsProxyCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        => ReloadableCommandRunner.Execute("DSCons.Revit.Starter.Commands.AlignTagsCommand", commandData, ref message, elements);
}

[Transaction(TransactionMode.Manual)]
public sealed class AboutMeProxyCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        => ReloadableCommandRunner.Execute("DSCons.Revit.Starter.Commands.AboutMeCommand", commandData, ref message, elements);
}

internal static class ReloadableCommandRunner
{
    private const string ProductAssemblyName = "DSCons.Revit.Starter.dll";
    private const string HostAssemblyName = "DSCons.Revit.HotReloadHost.dll";
    private static readonly object Gate = new object();

    internal static Result Execute(
        string commandTypeName,
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        lock (Gate)
        {
            try
            {
                var productPath = Path.Combine(GetDeploymentDirectory(), ProductAssemblyName);
                if (!File.Exists(productPath))
                {
                    message = "Không tìm thấy DLL Tool. Hãy chạy scripts\\build.ps1 đúng phiên bản Revit rồi bấm lại nút.";
                    return Result.Failed;
                }

                PreloadProductDependencies(GetDeploymentDirectory());
                var productAssembly = Assembly.Load(File.ReadAllBytes(productPath));
                var commandType = productAssembly.GetType(commandTypeName, false);
                if (commandType == null)
                {
                    message = "DLL Tool không chứa lệnh được yêu cầu. Nếu vừa thêm/đổi Ribbon command, hãy đóng Revit một lần, build lại Host và mở lại Revit.";
                    return Result.Failed;
                }

                var command = Activator.CreateInstance(commandType) as IExternalCommand;
                if (command == null)
                {
                    message = "Lệnh trong DLL Tool không triển khai IExternalCommand.";
                    return Result.Failed;
                }

                var productMessage = message;
                var result = command.Execute(commandData, ref productMessage, elements);
                message = productMessage;
                HotReloadLog.Write("Reloaded command: " + commandTypeName + ". Result: " + result + ".");
                return result;
            }
            catch (Exception exception)
            {
                HotReloadLog.Write(exception.ToString());
                message = "Không thể nạp lại DLL Tool. Xem log DSCons và gửi nguyên văn lỗi cho AI agent.";
                return Result.Failed;
            }
        }
    }

    private static string GetDeploymentDirectory()
    {
        var location = typeof(App).Assembly.Location;
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new InvalidOperationException("Hot Reload Host must be loaded from the development manifest.");
        }

        var directory = Path.GetDirectoryName(location);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Hot Reload Host deployment directory cannot be resolved.");
        }

        return directory;
    }

    private static void PreloadProductDependencies(string directory)
    {
        foreach (var candidatePath in Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(candidatePath);
            if (string.Equals(name, ProductAssemblyName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, HostAssemblyName, StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("RevitAPI", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                Assembly.Load(File.ReadAllBytes(candidatePath));
            }
            catch (BadImageFormatException)
            {
                // A native DLL is not a managed dependency. Revit owns native loading.
            }
        }
    }
}

internal static class HotReloadLog
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DSCons", "RevitApiKit", "hot-reload-host.log");

    internal static void Write(string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
                File.AppendAllText(LogPath, DateTime.Now.ToString("O") + " " + message + Environment.NewLine);
            }
        }
        catch
        {
            // Logging must never make Revit fail a command.
        }
    }
}