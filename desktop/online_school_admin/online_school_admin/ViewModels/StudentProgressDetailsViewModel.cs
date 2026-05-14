using System.Collections.ObjectModel;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class StudentProgressDetailsViewModel : BaseViewModel
{
    private readonly AdminProgressService _progress;
    private readonly int _enrollmentId;

    public StudentProgressDetailsViewModel(AdminProgressService progress, int enrollmentId)
    {
        _progress = progress;
        _enrollmentId = enrollmentId;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
    }

    public RelayCommand RefreshCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                RefreshCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    private AdminEnrollmentProgressDto? _details;
    public AdminEnrollmentProgressDto? Details { get => _details; private set => SetProperty(ref _details, value); }

    public ObservableCollection<AdminProgressModuleDto> Modules { get; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = await _progress.GetEnrollmentProgressAsync(_enrollmentId, cancellationToken);
            Details = dto;
            Modules.Clear();
            foreach (var m in dto.Modules.OrderBy(x => x.ModuleOrder))
                Modules.Add(m);
        }
        catch (ApiException ex)
        {
            Error = ApiErrorFormatter.Format(ex);
            Details = null;
            Modules.Clear();
        }
        catch (HttpRequestException)
        {
            Error = "Не удалось связаться с сервером.";
            Details = null;
            Modules.Clear();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
