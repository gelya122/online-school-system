namespace online_school_admin.ViewModels;

public sealed class PaymentsViewModel : BaseViewModel
{
    public OrdersTabViewModel Orders { get; }
    public PaymentsTabViewModel Payments { get; }
    public InstallmentsTabViewModel Installments { get; }

    public PaymentsViewModel(OrdersTabViewModel orders, PaymentsTabViewModel payments, InstallmentsTabViewModel installments)
    {
        Orders = orders;
        Payments = payments;
        Installments = installments;
    }
}

