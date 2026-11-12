namespace Hospital.Business;

/// <summary>Нарушенное правило валидации: свойство + описание ошибки.</summary>
public class BusinessRule
{
    public string Property { get; }
    public string Description { get; }

    public BusinessRule(string property, string description)
    {
        Property = property;
        Description = description;
    }

    public override string ToString() => Description;
}
