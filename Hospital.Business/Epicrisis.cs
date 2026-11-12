using System.Text;

namespace Hospital.Business;

/// <summary>Выписной эпикриз: персональные данные, период пребывания, диагноз, выполненные назначения.</summary>
public class Epicrisis
{
    public Patient Patient { get; }
    public MedicalRecord Record { get; }
    public string DepartmentName { get; }
    public string AttendingDoctorName { get; }

    public Epicrisis(Patient patient, MedicalRecord record, string departmentName, string attendingDoctorName)
    {
        Patient = patient;
        Record = record;
        DepartmentName = departmentName;
        AttendingDoctorName = attendingDoctorName;
    }

    public string ToText()
    {
        var sb = new StringBuilder();
        var from = Record.AdmissionDate;
        var to = Record.DischargeDate ?? DateTime.Now;
        int days = Math.Max(1, (to.Date - from.Date).Days + 1);

        sb.AppendLine("ВЫПИСНОЙ ЭПИКРИЗ");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();
        sb.AppendLine("Персональные данные пациента");
        sb.AppendLine($"  ФИО:             {Patient.FullName}");
        sb.AppendLine($"  Дата рождения:   {Patient.BirthDate:dd.MM.yyyy}");
        sb.AppendLine($"  Пол:             {Patient.Gender.ToRu()}");
        if (!string.IsNullOrWhiteSpace(Patient.Address)) sb.AppendLine($"  Адрес:           {Patient.Address}");
        if (!string.IsNullOrWhiteSpace(Patient.Phone)) sb.AppendLine($"  Телефон:         {Patient.Phone}");
        sb.AppendLine();
        sb.AppendLine("Пребывание в стационаре");
        sb.AppendLine($"  Период:          {from:dd.MM.yyyy} — {to:dd.MM.yyyy} ({days} дн.)");
        sb.AppendLine($"  Отделение:       {DepartmentName}");
        sb.AppendLine($"  Лечащий врач:    {AttendingDoctorName}");
        sb.AppendLine();
        sb.AppendLine("Диагноз при поступлении");
        sb.AppendLine($"  {Record.AdmissionDiagnosis}");
        sb.AppendLine();
        sb.AppendLine("Выполненные назначения");

        var done = Record.Prescriptions
            .Where(p => p.IsPerformed)
            .OrderBy(p => p.PerformedAt)
            .ToList();

        if (done.Count == 0)
        {
            sb.AppendLine("  (выполненных назначений нет)");
        }
        else
        {
            for (int i = 0; i < done.Count; i++)
            {
                var p = done[i];
                sb.AppendLine($"  {i + 1}. {p.Describe()}");
                sb.AppendLine($"     выполнено {p.PerformedAt:dd.MM.yyyy HH:mm}, исполнитель: {p.PerformedByName}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Дата выписки: {to:dd.MM.yyyy HH:mm}");
        return sb.ToString();
    }
}
