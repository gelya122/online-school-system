using System.Windows;
using System.Windows.Controls;
using online_school_admin.Services;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class CourseEditorWindow : Window
{
    public CourseEditorWindow(AuthApiService api, int courseId)
    {
        InitializeComponent();
        var vm = new CourseEditorViewModel(api, courseId);
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }

    private void Close_OnClick(object sender, RoutedEventArgs e) => Close();

    private void StructureTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is CourseEditorViewModel vm)
            vm.SelectedStructureNode = e.NewValue as CourseStructureNode;
    }
}
