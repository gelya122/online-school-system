using System.Windows;
using online_school_admin.Models;

namespace online_school_admin.Views;

public partial class StudentProfileWindow : Window
{
    public StudentProfileWindow(StudentListItem student)
    {
        InitializeComponent();
        DataContext = student;
    }
}
