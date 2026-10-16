using System;
using System.Windows;
using System.Windows.Interop;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;

namespace DSCons.Revit.Starter.Views;

/// <summary>
/// Modal WPF window for managing personal working views (Floor Plans and 3D View).
/// </summary>
public partial class WorkingViewsWindow : Window
{
    public WorkingViewsWindow(WorkingViewsViewModel viewModel, IntPtr revitMainWindowHandle)
    {
        InitializeComponent();
        WindowTheme.Apply(this);
        DataContext = viewModel;

        viewModel.RequestClose += () => Close();

        if (revitMainWindowHandle != IntPtr.Zero)
        {
            new WindowInteropHelper(this).Owner = revitMainWindowHandle;
        }
    }
}