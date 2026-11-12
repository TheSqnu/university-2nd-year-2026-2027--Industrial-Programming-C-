using Hospital.Business;

namespace Hospital.Data;

/// <summary>Содержимое файла данных (формат хранения — JSON).</summary>
public class HospitalData
{
    public List<Patient> Patients { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public List<Nurse> Nurses { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<MedicalRecord> Records { get; set; } = new();
}
