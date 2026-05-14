using System.Windows;
using System.Windows.Controls;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class CourseStructureView : UserControl
{
    public CourseStructureView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        BindSelectionForwarding();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        BindSelectionForwarding();

    private void BindSelectionForwarding()
    {
        StructureTree.SelectedItemChanged -= StructureTree_OnSelectedItemChanged;
        StructureTree.SelectedItemChanged += StructureTree_OnSelectedItemChanged;
    }

    private void StructureTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is CourseStructureViewModel vm)
            vm.SelectedTreeItem = e.NewValue;
    }
}
