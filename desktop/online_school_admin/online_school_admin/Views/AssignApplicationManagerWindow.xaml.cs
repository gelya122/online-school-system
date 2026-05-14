using System.Linq;
using System.Windows;
using online_school_admin.Models;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class AssignApplicationManagerWindow : Window
{
    public int? SelectedManagerId { get; private set; }
    public string? Note { get; private set; }

    public AssignApplicationManagerWindow(string clientName, string? currentManagerName, IReadOnlyList<IdTitleOption> managers)
    {
        InitializeComponent();
        ClientNameBlock.Text = clientName;
        CurrentManagerBlock.Text = string.IsNullOrWhiteSpace(currentManagerName) ? "— не назначен" : currentManagerName;
        foreach (var m in managers.Where(x => x.Id > 0))
            ManagerCombo.Items.Add(m);
        if (ManagerCombo.Items.Count > 0)
            ManagerCombo.SelectedIndex = 0;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (ManagerCombo.SelectedItem is not IdTitleOption opt || opt.Id <= 0)
        {
            MessageBox.Show(this, "Выберите менеджера.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedManagerId = opt.Id;
        var n = NoteBox.Text?.Trim();
        Note = string.IsNullOrEmpty(n) ? null : n;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
