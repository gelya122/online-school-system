using System.Windows;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class HomeworkAnswerReviewWindow : Window
{
    public HomeworkAnswerReviewWindow(HomeworkAnswerReviewWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.Saved += () =>
        {
            DialogResult = true;
            Close();
        };
        vm.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}

