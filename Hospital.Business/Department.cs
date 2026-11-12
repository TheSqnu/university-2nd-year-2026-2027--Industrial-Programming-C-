namespace Hospital.Business;

public class Department : BusinessObject
{
    public string Name { get; set; } = "";

    protected override void ValidateRules() =>
        Require(!string.IsNullOrWhiteSpace(Name), nameof(Name), "Укажите название отделения.");

    public override string ToString() => Name;
}
