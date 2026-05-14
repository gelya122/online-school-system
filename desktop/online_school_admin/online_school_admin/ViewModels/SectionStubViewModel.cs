namespace online_school_admin.ViewModels;

/// <summary>Заглушка раздела, подключённого к меню до появления полной реализации.</summary>
public sealed class SectionStubViewModel : BaseViewModel
{
    public SectionStubViewModel(string title, string? hint = null)
    {
        Title = title;
        Hint = hint ?? "Раздел подключён к навигации. Данные и действия будут добавлены позже.";
    }

    public string Title { get; }
    public string Hint { get; }
}
