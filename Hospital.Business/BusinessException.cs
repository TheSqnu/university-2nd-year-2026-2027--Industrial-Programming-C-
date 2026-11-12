namespace Hospital.Business;

/// <summary>Нарушение бизнес-правила (ошибка валидации, недопустимая операция).</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
