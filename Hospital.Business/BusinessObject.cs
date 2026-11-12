using System.Text.Json.Serialization;

namespace Hospital.Business;

/// <summary>
/// Базовый класс всех бизнес-сущностей. Механизм валидации:
/// наследник переопределяет ValidateRules() и внутри через Require()/AddBrokenRule()
/// регистрирует нарушенные правила; Validate() очищает список, запускает проверку
/// и возвращает true, если нарушений нет.
/// </summary>
public abstract class BusinessObject
{
    private readonly List<BusinessRule> _brokenRules = new();

    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore]
    public IReadOnlyList<BusinessRule> BrokenRules => _brokenRules;

    public bool Validate()
    {
        _brokenRules.Clear();
        ValidateRules();
        return _brokenRules.Count == 0;
    }

    protected abstract void ValidateRules();

    protected void AddBrokenRule(string property, string description) =>
        _brokenRules.Add(new BusinessRule(property, description));

    protected void Require(bool condition, string property, string description)
    {
        if (!condition) AddBrokenRule(property, description);
    }

    public string GetErrors() =>
        string.Join(Environment.NewLine, _brokenRules.Select(r => "• " + r.Description));
}
