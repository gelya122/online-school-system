using System.Windows;
using online_school_admin.Services;
using online_school_admin.ViewModels;

namespace online_school_admin.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow(AuthService auth)
    {
        InitializeComponent();
        DataContext = new RegisterViewModel(auth);
        Loaded += async (_, _) =>
        {
            if (DataContext is RegisterViewModel vm)
                await vm.LoadRolesAsync();
        };
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void PickAvatar_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
            vm.PickAvatarFile();
    }

    private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not RegisterViewModel vm)
            return;

        SaveButton.IsEnabled = false;
        try
        {
            var (ok, _) = await vm.TryRegisterAsync(PasswordBox.Password, ConfirmPasswordBox.Password);
            if (!ok)
                return;

            MessageBox.Show(this,
                "Сотрудник зарегистрирован. Теперь можно войти с этим email и паролем.",
                "Готово",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            DialogResult = true;
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }
}
