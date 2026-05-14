using System.Windows;
using online_school_admin.Models;

namespace online_school_admin.Views;

public partial class ApplicationStatusChangeWindow : Window
{
    public int? NewStatusId { get; private set; }
    public string? ReasonComment { get; private set; }

    private readonly int _currentStatusId;

    public ApplicationStatusChangeWindow(string clientLine, string phone, string? currentStatusName, int currentStatusId,
        IReadOnlyList<AdminApplicationStatusDictDto> statuses)
    {
        InitializeComponent();
        _currentStatusId = currentStatusId;
        ClientBlock.Text = clientLine;
        PhoneBlock.Text = string.IsNullOrWhiteSpace(phone) ? "—" : phone;
        CurrentStatusBlock.Text = string.IsNullOrWhiteSpace(currentStatusName) ? "—" : currentStatusName;
        foreach (var s in statuses)
        {
            if (s.StatusId != currentStatusId)
                StatusCombo.Items.Add(s);
        }

        if (StatusCombo.Items.Count > 0)
            StatusCombo.SelectedIndex = 0;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (StatusCombo.SelectedItem is not AdminApplicationStatusDictDto st)
        {
            MessageBox.Show(this, "Выберите новый статус.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (st.StatusId == _currentStatusId)
        {
            MessageBox.Show(this, "Новый статус должен отличаться от текущего.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        NewStatusId = st.StatusId;
        var r = ReasonBox.Text?.Trim();
        ReasonComment = string.IsNullOrEmpty(r) ? null : r;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
