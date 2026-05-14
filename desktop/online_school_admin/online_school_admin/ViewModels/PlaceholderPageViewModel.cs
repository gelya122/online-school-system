namespace online_school_admin.ViewModels;

public sealed class PlaceholderPageViewModel : BaseViewModel
{
    private string _title = "";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}

