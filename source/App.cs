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

        var panel = application.CreateRibbonPanel(tabName, "Revit API Kit");

        var workingViewButton = new PushButtonData(
            "DSConsWorkingView",
            "Working\n3D View",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.CreateWorkingViewCommand")
        {
            ToolTip = "Tạo hoặc kích hoạt View 3D làm việc cá nhân (3D_Working_HaiAnh) trên Project Browser.",
            Image = SemanticRibbonIconFactory.Create("view3d", false),
            LargeImage = SemanticRibbonIconFactory.Create("view3d", true)
        };

        var batchTagButton = new PushButtonData(
            "DSConsBatchTag",
            "Batch\nTag",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.BatchTagCommand")
        {
            ToolTip = "Ghi chú tag kích thước hàng loạt cho các đoạn ống gió trong View hiện hành.",
            Image = SemanticRibbonIconFactory.Create("tag", false),
            LargeImage = SemanticRibbonIconFactory.Create("tag", true)
        };

        var alignTagsButton = new PushButtonData(
            "DSConsAlignTags",
            "Align\nTags",
            typeof(App).Assembly.Location,
            "DSCons.Revit.Starter.Commands.AlignTagsCommand")
        {
            ToolTip = "Căn chỉnh các thẻ ghi chú (Tag) đã chọn thẳng hàng theo phương ngang hoặc phương dọc.",
            Image = SemanticRibbonIconFactory.Create("align", false),
            LargeImage = SemanticRibbonIconFactory.Create("align", true)
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

        panel.AddItem(workingViewButton);
        panel.AddItem(batchTagButton);
        panel.AddItem(alignTagsButton);
        panel.AddItem(aboutButton);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}