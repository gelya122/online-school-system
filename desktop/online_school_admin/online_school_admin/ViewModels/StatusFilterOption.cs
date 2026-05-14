namespace online_school_admin.ViewModels;

/// <summary>Элемент фильтра статуса потока: <see cref="Code"/> null — все.</summary>
public sealed class StatusFilterOption
{
    public StatusFilterOption(string? code, string label)
    {
        Code = code;
        Label = label;
    }

    public string? Code { get; }
    public string Label { get; }

    public override string ToString() => Label;
}
