using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Hospital.Business;

namespace Hospital.Data;

/// <summary>Загрузка и сохранение всех данных в один JSON-файл.</summary>
public class HospitalDataContext
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // чтобы кириллица читалась в файле
    };

    private readonly string _path;

    public HospitalData Data { get; private set; }

    public HospitalDataContext(string path)
    {
        _path = path;
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            Data = Seed();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_path, Encoding.UTF8);
            Data = JsonSerializer.Deserialize<HospitalData>(json, Options) ?? Seed();
        }
        catch (JsonException)
        {
            // файл повреждён: сохраняем копию и начинаем с начальных данных
            File.Copy(_path, _path + ".broken", overwrite: true);
            Data = Seed();
            Save();
        }
    }

    public void Save() =>
        File.WriteAllText(_path, JsonSerializer.Serialize(Data, Options), Encoding.UTF8);

    /// <summary>Начальные данные: отделения и персонал.</summary>
    private static HospitalData Seed()
    {
        var cardio = new Department { Name = "Кардиологическое" };
        var surgery = new Department { Name = "Хирургическое" };
        var neuro = new Department { Name = "Неврологическое" };

        var data = new HospitalData();
        data.Departments.AddRange(new[] { cardio, surgery, neuro });

        data.Doctors.AddRange(new[]
        {
            new Doctor { LastName = "Козлов", FirstName = "Дмитрий", Patronymic = "Андреевич", Specialty = "Терапевт (приёмное отделение)" },
            new Doctor { LastName = "Иванов", FirstName = "Иван", Patronymic = "Иванович", Specialty = "Кардиолог", DepartmentId = cardio.Id },
            new Doctor { LastName = "Петрова", FirstName = "Анна", Patronymic = "Сергеевна", Specialty = "Хирург", DepartmentId = surgery.Id },
            new Doctor { LastName = "Сидоров", FirstName = "Павел", Patronymic = "Николаевич", Specialty = "Невролог", DepartmentId = neuro.Id }
        });

        data.Nurses.AddRange(new[]
        {
            new Nurse { LastName = "Смирнова", FirstName = "Елена", Patronymic = "Викторовна", DepartmentId = cardio.Id },
            new Nurse { LastName = "Новикова", FirstName = "Ольга", Patronymic = "Петровна", DepartmentId = surgery.Id },
            new Nurse { LastName = "Морозова", FirstName = "Наталья", Patronymic = "Ивановна", DepartmentId = neuro.Id }
        });

        return data;
    }
}
