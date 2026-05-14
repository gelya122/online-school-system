using System.Windows;
using online_school_admin.Services;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class CourseInstanceWindow : Window
{
    public CourseInstanceWindow(AuthApiService api, int instanceId)
    {
        InitializeComponent();
        var vm = new CourseInstanceEditorViewModel(api, instanceId);
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadAsync();
    }
}
