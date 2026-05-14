namespace online_school_admin.ViewModels;

public sealed class RoleOption
{
    public RoleOption(int roleId, string caption)
    {
        RoleId = roleId;
        Caption = caption;
    }

    public int RoleId { get; }
    public string Caption { get; }
}
