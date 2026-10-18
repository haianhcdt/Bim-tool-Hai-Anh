using System;
using System.Windows;
using System.Windows.Interop;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;

namespace DSCons.Revit.Starter.Views;

/// <summary>
/// Interaction logic for Floor3DViewWindow.xaml
/// </summary>
public partial class Floor3DViewWindow : Window
{
    public Floor3DViewWindow(Floor3DViewViewModel viewModel, IntPtr revitMainWindowHandle)
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