using Hospital.Business;
using Hospital.Service;

namespace Hospital.App;

internal sealed class PatientForm : DialogBase
{
    private readonly TextBox _last = new(), _first = new(), _patronymic = new(), _phone = new();
    private readonly TextBox _address = new() { Multiline = true };
    private readonly DateTimePicker _birth = new() { Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today };
    private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public PatientForm(HospitalService svc, Patient existing = null)
        : base(existing == null ? "Новый пациент" : "Редактирование пациента")
    {
        _gender.Items.AddRange(new object[] { "Мужской", "Женский" });

        if (existing != null)
        {
            _last.Text = existing.LastName;
            _first.Text = existing.FirstName;
            _patronymic.Text = existing.Patronymic;
            _phone.Text = existing.Phone;
            _address.Text = existing.Address;
            var date = existing.BirthDate;
            if (date < _birth.MinDate) date = _birth.MinDate;
            if (date > _birth.MaxDate) date = _birth.MaxDate;
            _birth.Value = date;
            _gender.SelectedIndex = existing.Gender == Gender.Female ? 1 : 0;
        }
        else
        {
            _birth.Value = DateTime.Today.AddYears(-30);
            _gender.SelectedIndex = 0;
        }

        AddRow("Фамилия:", _last);
        AddRow("Имя:", _first);
        AddRow("Отчество:", _patronymic);
        AddRow("Дата рождения:", _birth, width: 160);
        AddRow("Пол:", _gender, width: 160);
        AddRow("Телефон:", _phone);
        AddRow("Адрес:", _address, height: 60);

        AddButtons(() =>
        {
            var patient = new Patient
            {
                LastName = _last.Text.Trim(),
                FirstName = _first.Text.Trim(),
                Patronymic = _patronymic.Text.Trim(),
                BirthDate = _birth.Value.Date,
                Gender = _gender.SelectedIndex == 1 ? Gender.Female : Gender.Male,
                Phone = _phone.Text.Trim(),
                Address = _address.Text.Trim()
            };
            if (existing != null) patient.Id = existing.Id;

            return UiHelper.Try(this, () => svc.SavePatient(patient));
        });
    }
}
