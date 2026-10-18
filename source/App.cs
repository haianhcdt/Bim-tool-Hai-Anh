using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter;

public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        var tabName = StudentBranding.RibbonTabName;
        try { application.CreateRibbonTab(tabName); }
        catch { /* The tab already exists in this Revit session. */ }

        var panel = application.CreateRibbonPanel(tabName, "BIM TOOL");

        var createViewWorkingButton = new PushButtonData(
            "DSConsCreateViewWorking",
            "Creat view\nWorking",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.CreateViewWorkingCommand")
        {
            ToolTip = "Khởi tạo View làm việc 3-trong-1: Tự động tạo Mặt Bằng MEP (13 hệ kỹ thuật), Mặt Cắt MEP (Ver/Hoz né Grid, kéo qua mọi tầng) và View 3D theo từng tầng (đóng khung Section Box).",
            Image = SemanticRibbonIconFactory.Create("sheet", false),
            LargeImage = SemanticRibbonIconFactory.Create("sheet", true)
        };

        var cleanupButton = new PushButtonData(
            "DSConsModelCleanup",
            "Clean\nFile",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.ModelCleanupCommand")
        {
            ToolTip = "Dọn dẹp file bàn giao & Kiểm toán mô hình: Xóa CAD/Revit links, kiểm tra trùng Mark, xuất báo cáo Warnings, triệt tiêu warnings và Purge unused.",
            Image = SemanticRibbonIconFactory.Create("filter", false),
            LargeImage = SemanticRibbonIconFactory.Create("filter", true)
        };

        var aboutButton = new PushButtonData(
            "DSConsAboutMe",
            "About\nMe",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.AboutMeCommand")
        {
            ToolTip = "Xem thông tin branding của người sở hữu add-in.",
            Image = SemanticRibbonIconFactory.Create("about", false),
            LargeImage = SemanticRibbonIconFactory.Create("about", true)
        };

        panel.AddItem(createViewWorkingButton);
        panel.AddItem(cleanupButton);
        panel.AddItem(aboutButton);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}