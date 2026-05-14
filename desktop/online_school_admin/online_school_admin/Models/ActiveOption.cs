namespace online_school_admin.Models;

public sealed record ActiveOption(bool? Value, string Title)
{
    public override string ToString() => Title;
}

