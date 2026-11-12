using System.Text.Json.Serialization;

namespace Hospital.Business;

/// <summary>Электронная медицинская карта (ЭМК) — один случай госпитализации.</summary>
public class MedicalRecord : BusinessObject
{
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DutyDoctorId { get; set; }
    public Guid? AttendingDoctorId { get; set; }

    public DateTime AdmissionDate { get; set; } = DateTime.Now;
    public DateTime? DischargeDate { get; set; }

    public string AdmissionDiagnosis { get; set; } = "";
    public string InitialExamNotes { get; set; } = "";

    /// <summary>Лечение — набор назначений врача.</summary>
    public List<Prescription> Prescriptions { get; set; } = new();

    [JsonIgnore]
    public bool IsDischarged => DischargeDate.HasValue;

    protected override void ValidateRules()
    {
        Require(PatientId != Guid.Empty, nameof(PatientId), "Не указан пациент.");
        Require(DepartmentId != Guid.Empty, nameof(DepartmentId), "Не указано отделение.");
        Require(DutyDoctorId != Guid.Empty, nameof(DutyDoctorId), "Не указан дежурный врач.");
        Require(!string.IsNullOrWhiteSpace(AdmissionDiagnosis), nameof(AdmissionDiagnosis), "Укажите диагноз при поступлении.");
        if (DischargeDate.HasValue)
            Require(DischargeDate.Value >= AdmissionDate, nameof(DischargeDate), "Дата выписки не может быть раньше даты поступления.");
    }

    public void AssignAttendingDoctor(Doctor doctor)
    {
        EnsureNotDischarged();
        if (doctor.DepartmentId != DepartmentId)
            throw new BusinessException("Лечащим врачом может быть только врач того отделения, в которое направлен пациент.");
        AttendingDoctorId = doctor.Id;
    }

    public void AddPrescription(Prescription prescription)
    {
        EnsureNotDischarged();
        if (AttendingDoctorId == null)
            throw new BusinessException("Сначала закрепите за пациентом лечащего врача.");
        if (!prescription.Validate())
            throw new BusinessException(prescription.GetErrors());

        prescription.AssignedAt = DateTime.Now;
        Prescriptions.Add(prescription);
    }

    public void PerformPrescription(Guid prescriptionId, Employee employee, DateTime when)
    {
        EnsureNotDischarged();
        var prescription = Prescriptions.FirstOrDefault(p => p.Id == prescriptionId)
            ?? throw new BusinessException("Назначение не найдено.");
        prescription.MarkPerformed(employee, when);
    }

    public void Discharge(DateTime when)
    {
        EnsureNotDischarged();
        if (when < AdmissionDate)
            throw new BusinessException("Дата выписки не может быть раньше даты поступления.");
        DischargeDate = when;
    }

    private void EnsureNotDischarged()
    {
        if (IsDischarged)
            throw new BusinessException("Пациент уже выписан — карта закрыта для изменений.");
    }
}
