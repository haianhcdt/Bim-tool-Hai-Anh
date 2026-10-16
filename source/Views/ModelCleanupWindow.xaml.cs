using System;
using System.Windows;
using System.Windows.Interop;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;

namespace DSCons.Revit.Starter.Views;

/// <summary>
/// Interaction logic for ModelCleanupWindow.xaml
/// </summary>
public partial class ModelCleanupWindow : Window
{
    public ModelCleanupWindow(ModelCleanupViewModel viewModel, IntPtr revitMainWindowHandle)
    {
        InitializeComponent();
        WindowTheme.Apply(this);
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        viewModel.RequestClose += () => Close();

        if (revitMainWindowHandle != IntPtr.Zero)
        {
            new WindowInteropHelper(this).Owner = revitMainWindowHandle;
        }
    }
}
