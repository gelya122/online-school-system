namespace online_school_admin.Models;

public sealed record PeriodOption(string Id, string Title)
{
    public override string ToString() => Title;
}

