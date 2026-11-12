using Hospital.Business;
using Hospital.Service;

namespace Hospital.App;

internal sealed class StaffForm : DialogBase
{
    private readonly TextBox _last = new(), _first = new(), _patronymic = new(), _specialty = new();
    private readonly ComboBox _department = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public StaffForm(HospitalService svc, bool isDoctor)
        : base(isDoctor ? "Новый врач" : "Новая медсестра")
    {
        _department.Items.Add("(без отделения)");
        foreach (var d in svc.GetDepartments()) _department.Items.Add(d);
        _department.SelectedIndex = 0;

        AddRow("Фамилия:", _last);
        AddRow("Имя:", _first);
        AddRow("Отчество:", _patronymic);
        if (isDoctor) AddRow("Специальность:", _specialty);
        AddRow("Отделение:", _department);

        AddButtons(() =>
        {
            Guid? departmentId = (_department.SelectedItem as Department)?.Id;

            return UiHelper.Try(this, () =>
            {
                if (isDoctor)
                {
                    svc.AddDoctor(new Doctor
                    {
                        LastName = _last.Text.Trim(),
                        FirstName = _first.Text.Trim(),
                        Patronymic = _patronymic.Text.Trim(),
                        Specialty = _specialty.Text.Trim(),
                        DepartmentId = departmentId
                    });
                }
                else
                {
                    svc.AddNurse(new Nurse
                    {
                        LastName = _last.Text.Trim(),
                        FirstName = _first.Text.Trim(),
                        Patronymic = _patronymic.Text.Trim(),
                        DepartmentId = departmentId
                    });
                }
            });
        });
    }
}
