using System.Text.Json.Serialization;

namespace Hospital.Business;

/// <summary>Сотрудник больницы (врач или медсестра).</summary>
public abstract class Employee : Person
{
    /// <summary>Отделение, в котором работает сотрудник (null — например, дежурный врач приёмного отделения).</summary>
    public Guid? DepartmentId { get; set; }

    [JsonIgnore]
    public abstract string Position { get; }

    public override string ToString() => $"{FullName} ({Position})";
}

public class Doctor : Employee
{
    public string Specialty { get; set; } = "";

    [JsonIgnore]
    public override string Position => string.IsNullOrWhiteSpace(Specialty) ? "врач" : Specialty;

    protected override void ValidateRules()
    {
        ValidatePerson();
        Require(!string.IsNullOrWhiteSpace(Specialty), nameof(Specialty), "Укажите специальность врача.");
    }
}

public class Nurse : Employee
{
    [JsonIgnore]
    public override string Position => "медсестра";

    protected override void ValidateRules() => ValidatePerson();
}
