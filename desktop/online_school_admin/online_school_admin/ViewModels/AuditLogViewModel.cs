using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using online_school_admin.Infrastructure;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class AuditLogViewModel : BaseViewModel
{
    private readonly AdminAuditLogService _audit;
    private readonly AdminEmployeesService _employees;

    public AuditLogViewModel(AdminAuditLogService audit, AdminEmployeesService employees)
    {
        _audit = audit;
        _employees = employees;
        RefreshCommand = new RelayCommand(async _ => { Skip = 0; await LoadAsync(); }, _ => !IsBusy);
        ExportCommand = new RelayCommand(_ => Export(), _ => !IsBusy && Rows.Count > 0);
        PrevPageCommand = new RelayCommand(async _ => { Skip = Math.Max(0, Skip - PageSize); await LoadAsync(); }, _ => !IsBusy && Skip > 0);
        NextPageCommand = new RelayCommand(async _ => { Skip += PageSize; await LoadAsync(); }, _ => !IsBusy && Skip + PageSize < TotalCount);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand PrevPageCommand { get; }
    public RelayCommand NextPageCommand { get; }

    public ObservableCollection<AdminEmployeeListRowDto> Employees { get; } = new();
    public ObservableCollection<AdminAuditLogListRowDto> Rows { get; } = new();

    private AdminEmployeeListRowDto? _selectedEmployeeFilter;
    public AdminEmployeeListRowDto? SelectedEmployeeFilter
    {
        get => _selectedEmployeeFilter;
        set => SetProperty(ref _selectedEmployeeFilter, value);
    }

    public string UserIdFilterText { get => _userIdFilterText; set => SetProperty(ref _userIdFilterText, value); }
    private string _userIdFilterText = "";

    public string EntityTypeText { get => _entityTypeText; set => SetProperty(ref _entityTypeText, value); }
    private string _entityTypeText = "";

    public string FromText { get => _fromText; set => SetProperty(ref _fromText, value); }
    private string _fromText = "";

    public string ToText { get => _toText; set => SetProperty(ref _toText, value); }
    private string _toText = "";

    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
    private string _searchText = "";

    public int PageSize { get; } = 50;
    public int Skip { get => _skip; set => SetProperty(ref _skip, value); }
    private int _skip;

    public int TotalCount { get => _totalCount; private set => SetProperty(ref _totalCount, value); }
    private int _totalCount;

    public string PageInfo => $"{Skip + 1}–{Math.Min(Skip + Rows.Count, Skip + PageSize)} из {TotalCount}";

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                ExportCommand.RaiseCanExecuteChanged();
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var emps = await _employees.GetEmployeesAsync(null, null, cancellationToken);
            Employees.Clear();
            foreach (var e in emps.OrderBy(x => x.FullName))
                Employees.Add(e);
        }
        catch
        {
            /* фильтр сотрудников необязателен */
        }
        await LoadAsync(cancellationToken);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            int? empId = SelectedEmployeeFilter?.EmployeeId;
            int? uid = null;
            if (int.TryParse(UserIdFilterText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var u))
                uid = u;

            var from = string.IsNullOrWhiteSpace(FromText) ? null : FromText.Trim();
            var to = string.IsNullOrWhiteSpace(ToText) ? null : ToText.Trim();
            var entity = string.IsNullOrWhiteSpace(EntityTypeText) ? null : EntityTypeText.Trim();
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            var page = await _audit.GetPageAsync(empId, uid, entity, from, to, search, Skip, PageSize, cancellationToken);
            TotalCount = page.TotalCount;
            if (page.Items.Count == 0 && TotalCount > 0 && Skip > 0)
            {
                Skip = 0;
                page = await _audit.GetPageAsync(empId, uid, entity, from, to, search, Skip, PageSize, cancellationToken);
                TotalCount = page.TotalCount;
            }
            Rows.Clear();
            foreach (var r in page.Items)
                Rows.Add(r);
            OnPropertyChanged(nameof(PageInfo));
            PrevPageCommand.RaiseCanExecuteChanged();
            NextPageCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }

    private void Export()
    {
        if (Rows.Count == 0) return;
        CsvExporter.PromptSaveAndExport(Rows, "audit_log.csv",
            nameof(AdminAuditLogListRowDto.CreatedAt),
            nameof(AdminAuditLogListRowDto.EmployeeDisplay),
            nameof(AdminAuditLogListRowDto.UserDisplay),
            nameof(AdminAuditLogListRowDto.Action),
            nameof(AdminAuditLogListRowDto.EntityType),
            nameof(AdminAuditLogListRowDto.EntityId),
            nameof(AdminAuditLogListRowDto.OldValues),
            nameof(AdminAuditLogListRowDto.NewValues),
            nameof(AdminAuditLogListRowDto.IpAddress),
            nameof(AdminAuditLogListRowDto.UserAgent));
    }
}
