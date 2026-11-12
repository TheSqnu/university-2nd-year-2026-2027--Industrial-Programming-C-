using Hospital.Business;
using Hospital.Service;

namespace Hospital.App;

public sealed class MainForm : Form
{
    // Уровень представления общается только с уровнем сервисов
    private readonly HospitalService _svc = new();

    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TabPage _pagePatients = new("Пациенты");
    private readonly TabPage _pageAdmission = new("Госпитализация");
    private readonly TabPage _pageRecords = new("ЭМК и лечение");
    private readonly TabPage _pageStaff = new("Персонал и отделения");

    // Пациенты
    private DataGridView _gridPatients;

    // Госпитализация
    private ComboBox _cbAdmPatient, _cbAdmDoctor, _cbAdmDept;
    private TextBox _tbDiagnosis, _tbExam;

    // ЭМК
    private SplitContainer _splitRecords;
    private SplitContainer _splitDetails;
    private bool _splitInitialized;
    private DataGridView _gridRecords, _gridPrescriptions;
    private TextBox _tbRecordInfo;
    private ComboBox _cbAttending;
    private Button _btnAssign, _btnAddPrescription, _btnPerform, _btnDischarge, _btnEpicrisis;

    // Персонал
    private DataGridView _gridDepartments, _gridDoctors, _gridNurses;

    public MainForm()
    {
        Text = "Больница — информационная система";
        Width = 1280;
        Height = 780;
        MinimumSize = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildPatientsTab();
        BuildAdmissionTab();
        BuildRecordsTab();
        BuildStaffTab();

        _tabs.TabPages.AddRange(new[] { _pagePatients, _pageAdmission, _pageRecords, _pageStaff });
        Controls.Add(_tabs);

        _tabs.SelectedIndexChanged += (s, e) =>
        {
            InitSplitIfNeeded();
            RefreshAll();
        };

        RefreshAll();
    }

    // ================================================================ построение

    private void BuildPatientsTab()
    {
        _gridPatients = UiHelper.Grid("ФИО", "Дата рождения", "Пол", "Телефон", "Адрес");
        _gridPatients.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditPatient(); };

        var bar = UiHelper.ToolBar(
            UiHelper.Btn("Добавить…", (s, e) => AddPatient()),
            UiHelper.Btn("Изменить…", (s, e) => EditPatient()),
            UiHelper.Btn("Удалить", (s, e) => DeletePatient()),
            UiHelper.Btn("Госпитализировать…", (s, e) => GoToAdmission()));

        _pagePatients.Controls.Add(_gridPatients);
        _pagePatients.Controls.Add(bar);
    }

    private void BuildAdmissionTab()
    {
        var hint = new Label
        {
            Text = "Дежурный врач проводит первичный осмотр пациента, создаёт электронную медицинскую карту " +
                   "и направляет пациента в одно из отделений.",
            Dock = DockStyle.Top,
            Height = Font.Height * 3 + 14,
            Padding = new Padding(12, 10, 12, 0),
            TextAlign = ContentAlignment.TopLeft
        };

        // таблица занимает всю вкладку: поля растягиваются вместе с окном
        var table = UiHelper.FormTable();

        _cbAdmPatient = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cbAdmDoctor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cbAdmDept = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _tbDiagnosis = new TextBox();
        _tbExam = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };

        UiHelper.AddRow(table, "Пациент:", _cbAdmPatient);
        UiHelper.AddRow(table, "Дежурный врач:", _cbAdmDoctor);
        UiHelper.AddRow(table, "Отделение:", _cbAdmDept);
        UiHelper.AddRow(table, "Диагноз при поступлении:", _tbDiagnosis);
        UiHelper.AddRow(table, "Первичный осмотр:", _tbExam, height: 130);

        var btn = UiHelper.Btn("Госпитализировать и создать ЭМК", (s, e) => Admit());
        btn.Anchor = AnchorStyles.Left;
        int row = table.RowCount;
        table.RowCount = row + 1;
        table.Controls.Add(btn, 1, row);

        _pageAdmission.Controls.Add(table);
        _pageAdmission.Controls.Add(hint);
    }

    private void BuildRecordsTab()
    {
        _gridRecords = UiHelper.Grid("Пациент", "Отделение", "Поступил", "Выписан", "Статус", "Лечащий врач", "Диагноз при поступлении");
        _gridRecords.SelectionChanged += (s, e) => RefreshRecordDetails();

        _splitRecords = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            Panel1MinSize = 80,
            Panel2MinSize = 150
        };
        _splitRecords.Panel1.Controls.Add(_gridRecords);

        // нижняя часть: сверху вниз — выбор врача, информация, кнопки назначений, таблица назначений
        _gridPrescriptions = UiHelper.Grid("Тип", "Назначение", "Назначено", "Статус", "Исполнитель", "Выполнено");

        _btnAddPrescription = UiHelper.Btn("Добавить назначение…", (s, e) => AddPrescription());
        _btnPerform = UiHelper.Btn("Отметить выполнение…", (s, e) => PerformPrescription());
        _btnDischarge = UiHelper.Btn("Выписать пациента", (s, e) => Discharge());
        _btnEpicrisis = UiHelper.Btn("Выписной эпикриз…", (s, e) => ShowEpicrisis());
        var prescriptionBar = UiHelper.ToolBar(_btnAddPrescription, _btnPerform, _btnDischarge, _btnEpicrisis);

        _tbRecordInfo = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            BackColor = SystemColors.Window
        };

        _cbAttending = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
        _btnAssign = UiHelper.Btn("Закрепить врача", (s, e) => AssignDoctor());
        var attendingBar = UiHelper.ToolBar(
            new Label { Text = "Лечащий врач:", AutoSize = true, Margin = new Padding(3, 8, 3, 0) },
            _cbAttending,
            _btnAssign);

        // нижняя часть окна делится ещё раз: сверху врач и сведения о карте, снизу назначения;
        // границу можно перетащить, чтобы увидеть весь текст
        _splitDetails = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        _splitDetails.Panel1.Controls.Add(_tbRecordInfo);
        _splitDetails.Panel1.Controls.Add(attendingBar);
        _splitDetails.Panel2.Controls.Add(_gridPrescriptions);
        _splitDetails.Panel2.Controls.Add(prescriptionBar);
        _splitRecords.Panel2.Controls.Add(_splitDetails);

        _pageRecords.Controls.Add(_splitRecords);
    }

    private void BuildStaffTab()
    {
        _gridDepartments = UiHelper.Grid("Отделение", "Врачей", "Медсестёр");
        _gridDoctors = UiHelper.Grid("ФИО", "Специальность", "Отделение");
        _gridNurses = UiHelper.Grid("ФИО", "Отделение");

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        layout.Controls.Add(Section("Отделения", _gridDepartments,
            UiHelper.Btn("Добавить отделение…", (s, e) => AddDepartment())), 0, 0);
        layout.Controls.Add(Section("Врачи", _gridDoctors,
            UiHelper.Btn("Добавить врача…", (s, e) => AddStaff(true))), 1, 0);
        layout.Controls.Add(Section("Медсёстры", _gridNurses,
            UiHelper.Btn("Добавить медсестру…", (s, e) => AddStaff(false))), 2, 0);

        _pageStaff.Controls.Add(layout);
    }

    private static GroupBox Section(string title, DataGridView grid, Button addButton)
    {
        var box = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(6) };
        box.Controls.Add(grid);
        box.Controls.Add(UiHelper.ToolBar(addButton));
        return box;
    }

    private void InitSplitIfNeeded()
    {
        if (_splitInitialized || _tabs.SelectedTab != _pageRecords) return;

        int h = _splitRecords.Height;
        if (h < 380) return;

        _splitRecords.SplitterDistance = Math.Clamp((int)(h * 0.33), 80, h - 250);

        int h2 = _splitRecords.Panel2.Height;
        _splitDetails.Panel1MinSize = 60;
        _splitDetails.Panel2MinSize = 100;
        _splitDetails.SplitterDistance = Math.Clamp((int)(h2 * 0.30), 60, h2 - 100);

        _splitInitialized = true;
    }

    // ================================================================ обновление

    private void RefreshAll(Guid? selectRecord = null)
    {
        RefreshPatients();
        RefreshAdmission();
        RefreshRecords(selectRecord);
        RefreshStaff();
    }

    private void RefreshPatients()
    {
        var keep = UiHelper.Selected<Patient>(_gridPatients)?.Id;
        UiHelper.Fill(_gridPatients, _svc.GetPatients(),
            p => new object[] { p.FullName, p.BirthDate.ToString("dd.MM.yyyy"), p.Gender.ToRu(), p.Phone, p.Address },
            p => keep.HasValue && p.Id == keep.Value);
    }

    private void RefreshAdmission()
    {
        UiHelper.Refill(_cbAdmPatient, _svc.GetPatients());
        UiHelper.Refill(_cbAdmDoctor, _svc.GetDoctors());
        UiHelper.Refill(_cbAdmDept, _svc.GetDepartments());
    }

    private void RefreshRecords(Guid? select = null)
    {
        var keep = select ?? UiHelper.Selected<RecordSummary>(_gridRecords)?.Record.Id;
        UiHelper.Fill(_gridRecords, _svc.GetRecordSummaries(),
            r => new object[]
            {
                r.PatientName,
                r.DepartmentName,
                r.Record.AdmissionDate.ToString("dd.MM.yyyy HH:mm"),
                r.Record.DischargeDate?.ToString("dd.MM.yyyy HH:mm") ?? "—",
                r.StatusText,
                r.AttendingName,
                r.Record.AdmissionDiagnosis
            },
            r => keep.HasValue && r.Record.Id == keep.Value);

        if (_gridRecords.Rows.Count == 0) RefreshRecordDetails();
    }

    private void RefreshRecordDetails()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        if (summary == null)
        {
            _tbRecordInfo.Text = "Выберите электронную медицинскую карту в списке выше.";
            _cbAttending.Items.Clear();
            _gridPrescriptions.Rows.Clear();
            SetRecordButtons(false, false, false);
            return;
        }

        var record = summary.Record;

        _tbRecordInfo.Text = string.Join(Environment.NewLine,
            $"Пациент: {summary.PatientName}",
            $"Диагноз при поступлении: {record.AdmissionDiagnosis}",
            $"Первичный осмотр (дежурный врач {summary.DutyDoctorName}): " +
                (string.IsNullOrWhiteSpace(record.InitialExamNotes) ? "—" : record.InitialExamNotes));

        var doctors = _svc.GetDoctorsOfDepartment(record.DepartmentId);
        _cbAttending.Items.Clear();
        foreach (var d in doctors) _cbAttending.Items.Add(d);
        _cbAttending.SelectedItem = doctors.FirstOrDefault(d => d.Id == record.AttendingDoctorId);

        var keepPrescription = UiHelper.Selected<Prescription>(_gridPrescriptions)?.Id;
        UiHelper.Fill(_gridPrescriptions, record.Prescriptions.OrderBy(p => p.AssignedAt),
            p => new object[]
            {
                p.GetKind().ToRu(),
                p.Describe(),
                p.AssignedAt.ToString("dd.MM.yyyy HH:mm"),
                p.IsPerformed ? "Выполнено" : "Ожидает",
                p.IsPerformed ? p.PerformedByName : "—",
                p.PerformedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—"
            },
            p => keepPrescription.HasValue && p.Id == keepPrescription.Value);

        foreach (DataGridViewRow row in _gridPrescriptions.Rows)
            if (row.Tag is Prescription { IsPerformed: true })
                row.DefaultCellStyle.ForeColor = Color.SeaGreen;

        SetRecordButtons(true, !record.IsDischarged, record.AttendingDoctorId.HasValue);
    }

    private void SetRecordButtons(bool hasRecord, bool active, bool hasAttending)
    {
        _cbAttending.Enabled = hasRecord && active;
        _btnAssign.Enabled = hasRecord && active;
        _btnAddPrescription.Enabled = hasRecord && active && hasAttending;
        _btnPerform.Enabled = hasRecord && active;
        _btnDischarge.Enabled = hasRecord && active;
        _btnEpicrisis.Enabled = hasRecord && !active;
    }

    private void RefreshStaff()
    {
        var departments = _svc.GetDepartments();
        var doctors = _svc.GetDoctors();
        var nurses = _svc.GetNurses();

        UiHelper.Fill(_gridDepartments, departments,
            d => new object[]
            {
                d.Name,
                doctors.Count(x => x.DepartmentId == d.Id),
                nurses.Count(x => x.DepartmentId == d.Id)
            });
        UiHelper.Fill(_gridDoctors, doctors,
            d => new object[] { d.FullName, d.Specialty, _svc.GetDepartmentName(d.DepartmentId) });
        UiHelper.Fill(_gridNurses, nurses,
            n => new object[] { n.FullName, _svc.GetDepartmentName(n.DepartmentId) });
    }

    // ================================================================ пациенты

    private void AddPatient()
    {
        using var form = new PatientForm(_svc);
        if (form.ShowDialog(this) == DialogResult.OK) RefreshAll();
    }

    private void EditPatient()
    {
        var patient = UiHelper.Selected<Patient>(_gridPatients);
        if (patient == null) { UiHelper.Info(this, "Выберите пациента в списке."); return; }

        using var form = new PatientForm(_svc, patient);
        if (form.ShowDialog(this) == DialogResult.OK) RefreshAll();
    }

    private void DeletePatient()
    {
        var patient = UiHelper.Selected<Patient>(_gridPatients);
        if (patient == null) { UiHelper.Info(this, "Выберите пациента в списке."); return; }
        if (!UiHelper.Confirm(this, $"Удалить пациента «{patient.FullName}»?")) return;

        if (UiHelper.Try(this, () => _svc.DeletePatient(patient.Id))) RefreshAll();
    }

    private void GoToAdmission()
    {
        var patient = UiHelper.Selected<Patient>(_gridPatients);
        if (patient == null) { UiHelper.Info(this, "Выберите пациента в списке."); return; }

        RefreshAdmission();
        _cbAdmPatient.SelectedItem = _cbAdmPatient.Items.Cast<Patient>().FirstOrDefault(p => p.Id == patient.Id);
        _tabs.SelectedTab = _pageAdmission;
    }

    // ================================================================ госпитализация

    private void Admit()
    {
        if (_cbAdmPatient.SelectedItem is not Patient patient ||
            _cbAdmDoctor.SelectedItem is not Doctor duty ||
            _cbAdmDept.SelectedItem is not Department department)
        {
            UiHelper.Info(this, "Выберите пациента, дежурного врача и отделение.");
            return;
        }

        MedicalRecord record = null;
        bool ok = UiHelper.Try(this, () =>
            record = _svc.AdmitPatient(patient.Id, duty.Id, department.Id, _tbDiagnosis.Text.Trim(), _tbExam.Text.Trim()));
        if (!ok) return;

        _tbDiagnosis.Clear();
        _tbExam.Clear();
        RefreshAll(record.Id);
        _tabs.SelectedTab = _pageRecords;
        UiHelper.Info(this, $"ЭМК создана. Пациент «{patient.FullName}» направлен в отделение «{department.Name}».\n" +
                            "Теперь закрепите за пациентом лечащего врача.");
    }

    // ================================================================ ЭМК и лечение

    private void AssignDoctor()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        if (summary == null) return;
        if (_cbAttending.SelectedItem is not Doctor doctor)
        {
            UiHelper.Info(this, "Выберите врача отделения.");
            return;
        }

        if (UiHelper.Try(this, () => _svc.AssignAttendingDoctor(summary.Record.Id, doctor.Id)))
            RefreshRecords();
    }

    private void AddPrescription()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        if (summary == null) return;

        using var form = new PrescriptionForm(_svc, summary.Record.Id, summary.PatientName);
        if (form.ShowDialog(this) == DialogResult.OK) RefreshRecords();
    }

    private void PerformPrescription()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        var prescription = UiHelper.Selected<Prescription>(_gridPrescriptions);
        if (summary == null) return;
        if (prescription == null) { UiHelper.Info(this, "Выберите назначение в таблице."); return; }
        if (prescription.IsPerformed) { UiHelper.Info(this, "Это назначение уже выполнено."); return; }

        var record = summary.Record;
        var performers = _svc.GetPerformersFor(prescription);
        object preselect = performers.FirstOrDefault(e => e.Id == record.AttendingDoctorId)
                           ?? performers.FirstOrDefault(e => e.DepartmentId == record.DepartmentId);

        string who = prescription is MedicationPrescription ? "Медсестра" : "Врач";
        using var form = new PickForm("Выполнение назначения", $"{who}-исполнитель:",
            performers.Cast<object>(), preselect);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        var performer = (Employee)form.Selected;
        if (UiHelper.Try(this, () => _svc.PerformPrescription(record.Id, prescription.Id, performer.Id)))
            RefreshRecords();
    }

    private void Discharge()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        if (summary == null) return;

        int pending = summary.Record.Prescriptions.Count(p => !p.IsPerformed);
        string message = $"Выписать пациента «{summary.PatientName}»?";
        if (pending > 0) message += $"\nНевыполненных назначений: {pending}.";
        if (!UiHelper.Confirm(this, message)) return;

        Epicrisis epicrisis = null;
        if (!UiHelper.Try(this, () => epicrisis = _svc.DischargePatient(summary.Record.Id))) return;

        RefreshAll(summary.Record.Id);
        ShowEpicrisisForm(epicrisis);
    }

    private void ShowEpicrisis()
    {
        var summary = UiHelper.Selected<RecordSummary>(_gridRecords);
        if (summary == null) return;

        Epicrisis epicrisis = null;
        if (UiHelper.Try(this, () => epicrisis = _svc.GetEpicrisis(summary.Record.Id)))
            ShowEpicrisisForm(epicrisis);
    }

    private void ShowEpicrisisForm(Epicrisis epicrisis)
    {
        using var form = new TextViewForm("Выписной эпикриз", epicrisis.ToText(),
            $"epicrisis_{epicrisis.Patient.LastName}.txt");
        form.ShowDialog(this);
    }

    // ================================================================ персонал

    private void AddDepartment()
    {
        using var form = new PromptForm("Новое отделение", "Название:", _svc.AddDepartment);
        if (form.ShowDialog(this) == DialogResult.OK) RefreshAll();
    }

    private void AddStaff(bool isDoctor)
    {
        using var form = new StaffForm(_svc, isDoctor);
        if (form.ShowDialog(this) == DialogResult.OK) RefreshAll();
    }
}
