using Hospital.Business;

namespace Hospital.Data;

public interface IDao<T> where T : BusinessObject
{
    IReadOnlyList<T> GetAll();
    T GetById(Guid id);
    void Add(T item);
    void Update(T item);
    void Delete(Guid id);
}

public interface IPatientDao : IDao<Patient> { }
public interface IDoctorDao : IDao<Doctor> { }
public interface INurseDao : IDao<Nurse> { }
public interface IDepartmentDao : IDao<Department> { }

public interface IMedicalRecordDao : IDao<MedicalRecord>
{
    IReadOnlyList<MedicalRecord> GetByPatient(Guid patientId);
}
