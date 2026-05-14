using online_school_admin.ViewModels;

namespace online_school_admin.Services;

public sealed class NavigationService : BaseViewModel
{
    private BaseViewModel? _currentViewModel;

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public void Navigate(BaseViewModel viewModel)
    {
        CurrentViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}

