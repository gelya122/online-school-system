using System.Globalization;
using System.Net;
using System.Net.Http;
using online_school_admin.Models;
using online_school_admin.Services;

namespace online_school_admin.ViewModels;

public sealed class OrderCreateViewModel : BaseViewModel
{
    private readonly AdminPaymentsService _pay;

    public OrderCreateViewModel(AdminPaymentsService pay)
    {
        _pay = pay;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(), _ => !IsBusy);
    }

    public event Action? Saved;
    public event Action? CancelRequested;

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _error;
    public string? Error { get => _error; set => SetProperty(ref _error, value); }

    public string StudentIdText { get => _studentIdText; set => SetProperty(ref _studentIdText, value); }
    private string _studentIdText = "";

    public string CourseIdText { get => _courseIdText; set => SetProperty(ref _courseIdText, value); }
    private string _courseIdText = "";

    public string InstanceIdText { get => _instanceIdText; set => SetProperty(ref _instanceIdText, value); }
    private string _instanceIdText = "";

    public string QuantityText { get => _quantityText; set => SetProperty(ref _quantityText, value); }
    private string _quantityText = "1";

    public string MethodIdText { get => _methodIdText; set => SetProperty(ref _methodIdText, value); }
    private string _methodIdText = "";

    /// <summary>Подставляет studentId и courseId из карточки заявки (instanceId опционально).</summary>
    public void PrefillFromApplication(int studentId, int? courseId, int? instanceId = null)
    {
        StudentIdText = studentId.ToString(CultureInfo.InvariantCulture);
        CourseIdText = courseId?.ToString(CultureInfo.InvariantCulture) ?? "";
        InstanceIdText = instanceId?.ToString(CultureInfo.InvariantCulture) ?? "";
    }

    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Error = null;
        IsBusy = true;
        try
        {
            if (!int.TryParse(StudentIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var studentId))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный studentId");
            if (!int.TryParse(CourseIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var courseId))
                throw new ApiException(HttpStatusCode.BadRequest, "Некорректный courseId");

            int? instanceId = null;
            if (!string.IsNullOrWhiteSpace(InstanceIdText))
            {
                if (!int.TryParse(InstanceIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ii))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный instanceId");
                instanceId = ii;
            }

            var qty = 1;
            if (!string.IsNullOrWhiteSpace(QuantityText))
            {
                if (!int.TryParse(QuantityText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out qty))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный quantity");
                if (qty < 1) qty = 1;
            }

            int? methodId = null;
            if (!string.IsNullOrWhiteSpace(MethodIdText))
            {
                if (!int.TryParse(MethodIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var mi))
                    throw new ApiException(HttpStatusCode.BadRequest, "Некорректный methodId");
                methodId = mi;
            }

            await _pay.CreateOrderAsync(new AdminOrderCreateDto
            {
                StudentId = studentId,
                CourseId = courseId,
                InstanceId = instanceId,
                Quantity = qty,
                MethodId = methodId
            }, cancellationToken);

            Saved?.Invoke();
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Не удалось связаться с сервером."; }
        finally { IsBusy = false; }
    }
}

