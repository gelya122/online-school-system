namespace online_school_admin.ViewModels;

public sealed class StudentClassFilterOption
{
    public StudentClassFilterOption(int? value, string caption)
    {
        Value = value;
        Caption = caption;
    }

    public int? Value { get; }
    public string Caption { get; }
}
