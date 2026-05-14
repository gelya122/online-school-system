using System.Windows;
using System.Windows.Controls;

namespace online_school_admin.Infrastructure;

public static class UserDialogs
{
    public static Window? TryGetOwner()
        => Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
           ?? Application.Current?.MainWindow;

    public static bool Confirm(string message, string title = "Подтверждение")
    {
        var owner = TryGetOwner();
        var r = owner != null
            ? MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
            : MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return r == MessageBoxResult.Yes;
    }

    public static void Info(string message, string title = "Сообщение")
    {
        var owner = TryGetOwner();
        if (owner != null)
            MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static void Warning(string message, string title = "Внимание")
    {
        var owner = TryGetOwner();
        if (owner != null)
            MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        else
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>Многострочный ввод (например, причина смены статуса). Отмена — null.</summary>
    public static string? PromptMultiline(string message, string title)
    {
        var tb = new TextBox
        {
            MinWidth = 420,
            Margin = new Thickness(0, 8, 0, 0),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Height = 100
        };

        var btnOk = new Button { Content = "Отправить", IsDefault = true, Width = 110, Margin = new Thickness(0, 12, 8, 0) };
        var btnCancel = new Button { Content = "Отмена", IsCancel = true, Width = 110, Margin = new Thickness(0, 12, 0, 0) };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(btnCancel);
        buttons.Children.Add(btnOk);

        var root = new StackPanel { Margin = new Thickness(14) };
        root.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });
        root.Children.Add(tb);
        root.Children.Add(buttons);

        var w = new Window
        {
            Title = title,
            Content = root,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = TryGetOwner(),
            MinWidth = 480,
            ResizeMode = ResizeMode.NoResize
        };

        string? captured = null;
        btnOk.Click += (_, _) =>
        {
            captured = tb.Text;
            w.DialogResult = true;
        };

        return w.ShowDialog() == true ? captured?.Trim() : null;
    }
}
