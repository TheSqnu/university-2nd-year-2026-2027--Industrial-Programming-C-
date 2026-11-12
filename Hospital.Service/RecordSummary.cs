using Hospital.Business;

namespace Hospital.Service;

/// <summary>Строка для отображения ЭМК в списке (ЭМК + имена связанных объектов).</summary>
public class RecordSummary
{
    public MedicalRecord Record { get; set; }
    public string PatientName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string DutyDoctorName { get; set; } = "";
    public string AttendingName { get; set; } = "";

    public string StatusText => Record.IsDischarged ? "Выписан" : "На лечении";
}
