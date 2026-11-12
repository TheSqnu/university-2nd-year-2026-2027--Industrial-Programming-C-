namespace Hospital.App;

/// <summary>Выбор одного элемента из списка.</summary>
internal sealed class PickForm : DialogBase
{
    private readonly ComboBox _combo = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public object Selected { get; private set; }

    public PickForm(string title, string prompt, IEnumerable<object> items, object preselect = null) : base(title)
    {
        foreach (var item in items) _combo.Items.Add(item);
        if (preselect != null && _combo.Items.Contains(preselect)) _combo.SelectedItem = preselect;
        else if (_combo.Items.Count > 0) _combo.SelectedIndex = 0;

        AddRow(prompt, _combo);
        AddButtons(() =>
        {
            if (_combo.SelectedItem == null)
            {
                UiHelper.Info(this, "Выберите значение из списка.");
                return false;
            }
            Selected = _combo.SelectedItem;
            return true;
        });
    }
}
