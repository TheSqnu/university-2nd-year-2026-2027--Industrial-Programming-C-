using Hospital.Business;
using Hospital.Data;

namespace Hospital.Service;

/// <summary>
/// Уровень сервисов: фасад над бизнес-логикой и DAO.
/// Уровень представления работает только с этим классом.
/// </summary>
public class HospitalService
{
    private readonly IPatientDao _patients;
    private readonly IDoctorDao _doctors;
    private readonly INurseDao _nurses;
    private readonly IDepartmentDao _departments;
    private readonly IMedicalRecordDao _records;

    public HospitalService()
        : this(Path.Combine(AppContext.BaseDirectory, "hospital_data.json")) { }

    public HospitalService(string dataFile)
    {
        var context = new HospitalDataContext(dataFile);
        _patients = new PatientDao(context);
        _doctors = new DoctorDao(context);
        _nurses = new NurseDao(context);
        _departments = new DepartmentDao(context);
        _records = new MedicalRecordDao(context);
    }

    // ------------------------------------------------------------ пациенты

    public IReadOnlyList<Patient> GetPatients() =>
        _patients.GetAll().OrderBy(p => p.FullName).ToList();

    /// <summary>Добавляет нового пациента или обновляет существующего (по Id).</summary>
    public void SavePatient(Patient patient)
    {
        if (!patient.Validate())
            throw new BusinessException(patient.GetErrors());

        if (_patients.GetById(patient.Id) == null) _patients.Add(patient);
        else _patients.Update(patient);
    }

    public void DeletePatient(Guid patientId)
    {
        if (_records.GetByPatient(patientId).Count > 0)
            throw new BusinessException("Нельзя удалить пациента, для которого создана электронная медицинская карта.");
        _patients.Delete(patientId);
    }

    // -------------------------------------------------- отделения и персонал

    public IReadOnlyList<Department> GetDepartments() =>
        _departments.GetAll().OrderBy(d => d.Name).ToList();

    public void AddDepartment(string name)
    {
        var department = new Department { Name = name };
        if (!department.Validate())
            throw new BusinessException(department.GetErrors());
        if (_departments.GetAll().Any(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessException("Отделение с таким названием уже существует.");
        _departments.Add(department);
    }

    public string GetDepartmentName(Guid? id) =>
        id.HasValue ? _departments.GetById(id.Value)?.Name ?? "—" : "—";

    public IReadOnlyList<Doctor> GetDoctors() =>
        _doctors.GetAll().OrderBy(d => d.FullName).ToList();

    public IReadOnlyList<Doctor> GetDoctorsOfDepartment(Guid departmentId) =>
        GetDoctors().Where(d => d.DepartmentId == departmentId).ToList();

    public IReadOnlyList<Nurse> GetNurses() =>
        _nurses.GetAll().OrderBy(n => n.FullName).ToList();

    public void AddDoctor(Doctor doctor)
    {
        if (!doctor.Validate()) throw new BusinessException(doctor.GetErrors());
        _doctors.Add(doctor);
    }

    public void AddNurse(Nurse nurse)
    {
        if (!nurse.Validate()) throw new BusinessException(nurse.GetErrors());
        _nurses.Add(nurse);
    }

    /// <summary>Кто может выполнить назначение: медикаментозное — медсестры, остальные — врачи.</summary>
    public IReadOnlyList<Employee> GetPerformersFor(Prescription prescription)
    {
        if (prescription is MedicationPrescription)
            return GetNurses().Cast<Employee>().ToList();
        return GetDoctors().Cast<Employee>().ToList();
    }

    // ------------------------------------------------------ ЭМК и лечение

    public IReadOnlyList<RecordSummary> GetRecordSummaries() =>
        _records.GetAll()
            .OrderBy(r => r.IsDischarged)
            .ThenByDescending(r => r.AdmissionDate)
            .Select(ToSummary)
            .ToList();

    /// <summary>
    /// Госпитализация: дежурный врач проводит первичный осмотр,
    /// создаёт ЭМК и направляет пациента в отделение.
    /// </summary>
    public MedicalRecord AdmitPatient(Guid patientId, Guid dutyDoctorId, Guid departmentId,
                                      string diagnosis, string examNotes)
    {
        _ = _patients.GetById(patientId) ?? throw new BusinessException("Пациент не найден.");
        _ = _doctors.GetById(dutyDoctorId) ?? throw new BusinessException("Дежурный врач не найден.");
        _ = _departments.GetById(departmentId) ?? throw new BusinessException("Отделение не найдено.");

        if (_records.GetByPatient(patientId).Any(r => !r.IsDischarged))
            throw new BusinessException("Пациент уже госпитализирован: у него есть незакрытая ЭМК.");

        var record = new MedicalRecord
        {
            PatientId = patientId,
            DutyDoctorId = dutyDoctorId,
            DepartmentId = departmentId,
            AdmissionDate = DateTime.Now,
            AdmissionDiagnosis = diagnosis,
            InitialExamNotes = examNotes
        };

        if (!record.Validate())
            throw new BusinessException(record.GetErrors());

        _records.Add(record);
        return record;
    }

    /// <summary>Закрепляет за пациентом врача отделения.</summary>
    public void AssignAttendingDoctor(Guid recordId, Guid doctorId)
    {
        var record = GetRecord(recordId);
        var doctor = _doctors.GetById(doctorId) ?? throw new BusinessException("Врач не найден.");
        record.AssignAttendingDoctor(doctor);
        _records.Update(record);
    }

    public void AddPrescription(Guid recordId, Prescription prescription)
    {
        var record = GetRecord(recordId);
        record.AddPrescription(prescription);
        _records.Update(record);
    }

    /// <summary>Отмечает назначение выполненным; запись вносится в ЭМК.</summary>
    public void PerformPrescription(Guid recordId, Guid prescriptionId, Guid employeeId)
    {
        var record = GetRecord(recordId);
        Employee employee = (Employee)_doctors.GetById(employeeId) ?? _nurses.GetById(employeeId);
        if (employee == null) throw new BusinessException("Исполнитель не найден.");

        record.PerformPrescription(prescriptionId, employee, DateTime.Now);
        _records.Update(record);
    }

    /// <summary>Выписывает пациента и формирует выписной эпикриз.</summary>
    public Epicrisis DischargePatient(Guid recordId)
    {
        var record = GetRecord(recordId);
        record.Discharge(DateTime.Now);
        _records.Update(record);
        return BuildEpicrisis(record);
    }

    public Epicrisis GetEpicrisis(Guid recordId)
    {
        var record = GetRecord(recordId);
        if (!record.IsDischarged)
            throw new BusinessException("Эпикриз формируется после выписки пациента.");
        return BuildEpicrisis(record);
    }

    // ------------------------------------------------------------- внутреннее

    private MedicalRecord GetRecord(Guid id) =>
        _records.GetById(id) ?? throw new BusinessException("Электронная медицинская карта не найдена.");

    private Epicrisis BuildEpicrisis(MedicalRecord record)
    {
        var patient = _patients.GetById(record.PatientId);
        var attending = record.AttendingDoctorId.HasValue ? _doctors.GetById(record.AttendingDoctorId.Value) : null;
        return new Epicrisis(patient, record,
            GetDepartmentName(record.DepartmentId),
            attending?.FullName ?? "не назначен");
    }

    private RecordSummary ToSummary(MedicalRecord r)
    {
        var attending = r.AttendingDoctorId.HasValue ? _doctors.GetById(r.AttendingDoctorId.Value) : null;
        return new RecordSummary
        {
            Record = r,
            PatientName = _patients.GetById(r.PatientId)?.FullName ?? "—",
            DepartmentName = GetDepartmentName(r.DepartmentId),
            DutyDoctorName = _doctors.GetById(r.DutyDoctorId)?.FullName ?? "—",
            AttendingName = attending?.FullName ?? "не назначен"
        };
    }
}
