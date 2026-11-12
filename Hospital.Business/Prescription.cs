using System.Text.Json.Serialization;

namespace Hospital.Business;

/// <summary>Назначение врача (элемент лечения).</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(MedicationPrescription), "medication")]
[JsonDerivedType(typeof(DiagnosticPrescription), "diagnostic")]
[JsonDerivedType(typeof(PreventivePrescription), "preventive")]
public abstract class Prescription : BusinessObject
{
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    // Запись о выполнении (вносится в ЭМК)
    public bool IsPerformed { get; set; }
    public DateTime? PerformedAt { get; set; }
    public Guid? PerformedById { get; set; }
    public string PerformedByName { get; set; } = "";

    public abstract PrescriptionKind GetKind();
    public abstract string Describe();
    public abstract bool CanBePerformedBy(Employee employee);
    protected abstract string PerformerRule();

    public void MarkPerformed(Employee employee, DateTime when)
    {
        if (IsPerformed)
            throw new BusinessException("Назначение уже выполнено.");
        if (!CanBePerformedBy(employee))
            throw new BusinessException(PerformerRule());

        IsPerformed = true;
        PerformedAt = when;
        PerformedById = employee.Id;
        PerformedByName = employee.FullName;
    }
}

/// <summary>Медикаментозное назначение (инъекции, таблетки) — выполняет медсестра.</summary>
public class MedicationPrescription : Prescription
{
    public DosageForm DosageForm { get; set; }
    public string Name { get; set; } = "";

    /// <summary>Для инъекций — количество.</summary>
    public int InjectionCount { get; set; }

    /// <summary>Для таблеток — дозировка и количество дней приёма.</summary>
    public string Dosage { get; set; } = "";
    public int Days { get; set; }

    public override PrescriptionKind GetKind() => PrescriptionKind.Medication;

    public override string Describe() => DosageForm == DosageForm.Injection
        ? $"Инъекции: {Name}, количество: {InjectionCount}"
        : $"Таблетки: {Name}, дозировка: {Dosage}, дней приёма: {Days}";

    public override bool CanBePerformedBy(Employee employee) => employee is Nurse;

    protected override string PerformerRule() => "Медикаментозные назначения выполняет медсестра.";

    protected override void ValidateRules()
    {
        Require(!string.IsNullOrWhiteSpace(Name), nameof(Name), "Укажите название лекарства.");
        if (DosageForm == DosageForm.Injection)
        {
            Require(InjectionCount > 0, nameof(InjectionCount), "Количество инъекций должно быть больше нуля.");
        }
        else
        {
            Require(!string.IsNullOrWhiteSpace(Dosage), nameof(Dosage), "Укажите дозировку таблеток.");
            Require(Days > 0, nameof(Days), "Количество дней приёма должно быть больше нуля.");
        }
    }
}

/// <summary>Диагностическое назначение (анализ крови, УЗИ, КТ, ФГС...) — выполняет врач.</summary>
public class DiagnosticPrescription : Prescription
{
    public string Name { get; set; } = "";

    public override PrescriptionKind GetKind() => PrescriptionKind.Diagnostic;
    public override string Describe() => $"Диагностика: {Name}";
    public override bool CanBePerformedBy(Employee employee) => employee is Doctor;
    protected override string PerformerRule() => "Диагностические назначения выполняет врач.";

    protected override void ValidateRules() =>
        Require(!string.IsNullOrWhiteSpace(Name), nameof(Name), "Укажите вид исследования.");
}

/// <summary>Профилактическое назначение (массаж, ЛФК...) — выполняет врач.</summary>
public class PreventivePrescription : Prescription
{
    public string Name { get; set; } = "";

    public override PrescriptionKind GetKind() => PrescriptionKind.Preventive;
    public override string Describe() => $"Профилактика: {Name}";
    public override bool CanBePerformedBy(Employee employee) => employee is Doctor;
    protected override string PerformerRule() => "Профилактические назначения выполняет врач.";

    protected override void ValidateRules() =>
        Require(!string.IsNullOrWhiteSpace(Name), nameof(Name), "Укажите вид процедуры.");
}
