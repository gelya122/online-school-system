using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using online_school_admin.Models;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class HomeworkEditorView : UserControl
{
    public HomeworkEditorView()
    {
        InitializeComponent();
    }

    private void HomeworkTaskRowContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu ctx)
            return;
        if (FindDataGridRow(ctx.PlacementTarget) is { } row)
            ctx.Tag = row.Item;
        else
            ctx.Tag = null;
    }

    private static DataGridRow? FindDataGridRow(object? target)
    {
        if (target is DataGridRow r)
            return r;
        if (target is not DependencyObject d)
            return null;
        for (var o = d; o != null; o = VisualTreeHelper.GetParent(o))
        {
            if (o is DataGridRow row)
                return row;
        }
        return null;
    }

    private async void DeleteTaskMenu_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not HomeworkEditorViewModel vm)
            return;
        if (sender is not MenuItem { Parent: ContextMenu ctx })
            return;
        if (ctx.Tag is not AdminHomeworkTaskRowDto row)
            return;
        await vm.DeleteTaskForRowAsync(row);
    }
}
