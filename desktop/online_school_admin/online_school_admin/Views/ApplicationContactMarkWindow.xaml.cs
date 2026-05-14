using System.Windows;
using online_school_admin.Models;

namespace online_school_admin.Views;

public partial class ApplicationContactMarkWindow : Window
{
    public AdminApplicationContactPatchDto? Result { get; private set; }

    public ApplicationContactMarkWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var c = CommentBox.Text?.Trim();
        Result = new AdminApplicationContactPatchDto
        {
            Comment = string.IsNullOrEmpty(c) ? null : c
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
