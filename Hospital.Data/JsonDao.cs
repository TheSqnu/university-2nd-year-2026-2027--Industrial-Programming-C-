using Hospital.Business;

namespace Hospital.Data;

/// <summary>Общая реализация DAO поверх JSON-контекста.</summary>
public abstract class JsonDao<T> : IDao<T> where T : BusinessObject
{
    private readonly HospitalDataContext _context;
    private readonly Func<HospitalData, List<T>> _selector;

    protected JsonDao(HospitalDataContext context, Func<HospitalData, List<T>> selector)
    {
        _context = context;
        _selector = selector;
    }

    private List<T> Items => _selector(_context.Data);

    public IReadOnlyList<T> GetAll() => Items.ToList();

    public T GetById(Guid id) => Items.FirstOrDefault(x => x.Id == id);

    public void Add(T item)
    {
        Items.Add(item);
        _context.Save();
    }

    public void Update(T item)
    {
        int index = Items.FindIndex(x => x.Id == item.Id);
        if (index < 0) throw new KeyNotFoundException("Объект не найден в хранилище.");
        Items[index] = item;
        _context.Save();
    }

    public void Delete(Guid id)
    {
        Items.RemoveAll(x => x.Id == id);
        _context.Save();
    }
}

public class PatientDao : JsonDao<Patient>, IPatientDao
{
    public PatientDao(HospitalDataContext ctx) : base(ctx, d => d.Patients) { }
}

public class DoctorDao : JsonDao<Doctor>, IDoctorDao
{
    public DoctorDao(HospitalDataContext ctx) : base(ctx, d => d.Doctors) { }
}

public class NurseDao : JsonDao<Nurse>, INurseDao
{
    public NurseDao(HospitalDataContext ctx) : base(ctx, d => d.Nurses) { }
}

public class DepartmentDao : JsonDao<Department>, IDepartmentDao
{
    public DepartmentDao(HospitalDataContext ctx) : base(ctx, d => d.Departments) { }
}

public class MedicalRecordDao : JsonDao<MedicalRecord>, IMedicalRecordDao
{
    public MedicalRecordDao(HospitalDataContext ctx) : base(ctx, d => d.Records) { }

    public IReadOnlyList<MedicalRecord> GetByPatient(Guid patientId) =>
        GetAll().Where(r => r.PatientId == patientId).ToList();
}
