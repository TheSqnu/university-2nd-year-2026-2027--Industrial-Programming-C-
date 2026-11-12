using Hospital.Business;
using Hospital.Service;

namespace Hospital.App;

internal sealed class PrescriptionForm : DialogBase
{
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _name = new();
    private readonly NumericUpDown _count = new() { Minimum = 1, Maximum = 100, Value = 1 };
    private readonly TextBox _dosage = new();
    private readonly NumericUpDown _days = new() { Minimum = 1, Maximum = 365, Value = 7 };

    private Label _lblName, _lblCount, _lblDosage, _lblDays;

    public PrescriptionForm(HospitalService svc, Guid recordId, string patientName)
        : base($"Новое назначение — {patientName}")
    {
        _kind.Items.AddRange(new object[]
        {
            "Медикаментозное: инъекции",
            "Медикаментозное: таблетки",
            "Диагностическое (анализ, УЗИ, КТ, ФГС…)",
            "Профилактическое (массаж, ЛФК…)"
        });

        AddRow("Вид назначения:", _kind);
        _lblName = AddRow("Лекарство:", _name);
        _lblCount = AddRow("Количество инъекций:", _count, width: 100);
        _lblDosage = AddRow("Дозировка:", _dosage);
        _lblDays = AddRow("Дней приёма:", _days, width: 100);

        _kind.SelectedIndexChanged += (s, e) => UpdateFields();
        _kind.SelectedIndex = 0;

        AddButtons(() =>
        {
            string name = _name.Text.Trim();
            Prescription prescription = _kind.SelectedIndex switch
            {
                0 => new MedicationPrescription { DosageForm = DosageForm.Injection, Name = name, InjectionCount = (int)_count.Value },
                1 => new MedicationPrescription { DosageForm = DosageForm.Tablet, Name = name, Dosage = _dosage.Text.Trim(), Days = (int)_days.Value },
                2 => new DiagnosticPrescription { Name = name },
                _ => new PreventivePrescription { Name = name }
            };

            return UiHelper.Try(this, () => svc.AddPrescription(recordId, prescription));
        }, "Назначить");
    }

    private void UpdateFields()
    {
        int k = _kind.SelectedIndex;
        _lblName.Text = k <= 1 ? "Лекарство:" : k == 2 ? "Исследование:" : "Процедура:";

        SetVisible(_lblCount, _count, k == 0);
        SetVisible(_lblDosage, _dosage, k == 1);
        SetVisible(_lblDays, _days, k == 1);

        FitHeight();
    }

    private static void SetVisible(Control label, Control field, bool visible)
    {
        label.Visible = visible;
        field.Visible = visible;
    }
}
