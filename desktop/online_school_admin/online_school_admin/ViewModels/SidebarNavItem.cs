using CommunityToolkit.Mvvm.ComponentModel;

namespace online_school_admin.ViewModels;

/// <summary>Пункт левой панели навигации.</summary>
public partial class SidebarNavItem : ObservableObject
{
    public required string Id { get; init; }
    public required string Title { get; init; }

    [ObservableProperty]
    private string _subtitle = "";
}
