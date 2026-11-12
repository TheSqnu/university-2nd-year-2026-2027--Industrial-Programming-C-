using System.Text;

namespace Hospital.App;

/// <summary>Просмотр текстового документа (например, эпикриза) с сохранением в файл.</summary>
internal sealed class TextViewForm : Form
{
    public TextViewForm(string title, string text, string fileName)
    {
        Text = title;
        Size = new Size(720, 640);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var box = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10.5f),
            BackColor = SystemColors.Window,
            Text = text
        };

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(6)
        };
        bar.Controls.Add(UiHelper.Btn("Закрыть", (s, e) => Close()));
        bar.Controls.Add(UiHelper.Btn("Сохранить в файл…", (s, e) => Save(text, fileName)));

        Controls.Add(box);
        Controls.Add(bar);
        box.Select(0, 0);
    }

    private void Save(string text, string fileName)
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Текстовый файл (*.txt)|*.txt",
            FileName = fileName,
            Title = "Сохранить документ"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (UiHelper.Try(this, () => File.WriteAllText(dlg.FileName, text, Encoding.UTF8)))
            UiHelper.Info(this, "Файл сохранён.");
    }
}
