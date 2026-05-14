using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class ReviewsAdminViewModel : BaseViewModel
{
    private readonly AdminSettingsService _settings;

    public ReviewsAdminViewModel(AdminSettingsService settings)
    {
        _settings = settings;
        BackCommand = new RelayCommand(_ => BackRequested?.Invoke(), _ => !IsBusy);
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        PublishCommand = new RelayCommand(async _ => await SetPublishedAsync(true), _ => !IsBusy && Selected != null);
        HideCommand = new RelayCommand(async _ => await SetPublishedAsync(false), _ => !IsBusy && Selected != null);
    }

    public event Action? BackRequested;

    public RelayCommand BackCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand PublishCommand { get; }
    public RelayCommand HideCommand { get; }

    public ObservableCollection<AdminReviewListRowDto> Rows { get; } = new();

    private AdminReviewListRowDto? _selected;
    public AdminReviewListRowDto? Selected
    {
        get => _selected;
        set
        {
            if (SetProperty(ref _selected, value))
            {
                PublishCommand.RaiseCanExecuteChanged();
                HideCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                BackCommand.RaiseCanExecuteChanged();
                RefreshCommand.RaiseCanExecuteChanged();
                PublishCommand.RaiseCanExecuteChanged();
                HideCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _settings.GetReviewsAsync(cancellationToken);
            Rows.Clear();
            foreach (var r in list)
                Rows.Add(r);
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private async Task SetPublishedAsync(bool published)
    {
        if (Selected == null) return;
        Error = null;
        IsBusy = true;
        try
        {
            await _settings.SetReviewPublishedAsync(Selected.ReviewId, published);
            await LoadAsync();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}
