namespace Hospital.App;

/// <summary>Ввод одной строки (например, названия отделения).</summary>
internal sealed class PromptForm : DialogBase
{
    private readonly TextBox _text = new();

    public PromptForm(string title, string label, Action<string> save) : base(title)
    {
        AddRow(label, _text);
        AddButtons(() => UiHelper.Try(this, () => save(_text.Text.Trim())));
    }
}
