namespace Hospital.App;

/// <summary>
/// Базовая модальная форма: таблица полей + кнопки «ОК / Отмена».
/// Окно можно растягивать — поля подстраиваются под его размер.
/// </summary>
internal abstract class DialogBase : Form
{
    protected TableLayoutPanel Root { get; }

    protected DialogBase(string title)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;

        Root = UiHelper.FormTable();
        Controls.Add(Root);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // начальный размер — по содержимому; меньше этого окно уменьшить нельзя
        Size preferred = Root.GetPreferredSize(Size.Empty);
        ClientSize = preferred;
        MinimumSize = Size;
        CenterToParent();
    }

    /// <summary>Подгоняет высоту окна под содержимое (после скрытия/показа полей).</summary>
    protected void FitHeight()
    {
        if (!IsHandleCreated) return;

        MinimumSize = Size.Empty;
        ClientSize = new Size(ClientSize.Width, Root.GetPreferredSize(new Size(ClientSize.Width, 0)).Height);
        MinimumSize = Size;
    }

    protected Label AddRow(string label, Control field, int height = 0, int width = 340) =>
        UiHelper.AddRow(Root, label, field, height, width);

    /// <summary>onOk возвращает true, если данные приняты и окно можно закрыть.</summary>
    protected void AddButtons(Func<bool> onOk, string okText = "ОК")
    {
        var ok = UiHelper.Btn(okText, (s, e) => { if (onOk()) DialogResult = DialogResult.OK; });
        var cancel = UiHelper.Btn("Отмена", (s, e) => { });
        cancel.DialogResult = DialogResult.Cancel;

        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(3, 10, 3, 3)
        };
        panel.Controls.Add(cancel);
        panel.Controls.Add(ok);

        int row = Root.RowCount;
        Root.RowCount = row + 1;
        Root.Controls.Add(panel, 0, row);
        Root.SetColumnSpan(panel, 2);

        AcceptButton = ok;
        CancelButton = cancel;
    }
}
