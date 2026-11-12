using Hospital.Business;

namespace Hospital.App;

/// <summary>Вспомогательные методы для построения интерфейса.</summary>
internal static class UiHelper
{
    public static Button Btn(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6, 2, 6, 2),
            Margin = new Padding(3)
        };
        b.Click += onClick;
        return b;
    }

    public static FlowLayoutPanel ToolBar(params Control[] items)
    {
        var p = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(4)
        };
        p.Controls.AddRange(items);
        return p;
    }

    public static DataGridView Grid(params string[] headers)
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };
        foreach (var h in headers) g.Columns.Add(h, h);
        return g;
    }

    /// <summary>Заполняет таблицу; объект строки хранится в Row.Tag.</summary>
    public static void Fill<T>(DataGridView g, IEnumerable<T> items, Func<T, object[]> row, Func<T, bool> reselect = null)
    {
        g.Rows.Clear();
        int selected = -1;
        foreach (var item in items)
        {
            int i = g.Rows.Add(row(item));
            g.Rows[i].Tag = item;
            if (reselect != null && reselect(item)) selected = i;
        }

        if (g.Rows.Count == 0) return;
        if (selected < 0) selected = 0;
        g.ClearSelection();
        g.Rows[selected].Selected = true;
        g.CurrentCell = g.Rows[selected].Cells[0];
    }

    public static T Selected<T>(DataGridView g) where T : class => g.CurrentRow?.Tag as T;

    /// <summary>Заново заполняет список, сохраняя выбранный элемент (по Id).</summary>
    public static void Refill<T>(ComboBox cb, IEnumerable<T> items) where T : BusinessObject
    {
        var keep = (cb.SelectedItem as T)?.Id;
        cb.BeginUpdate();
        cb.Items.Clear();
        foreach (var item in items) cb.Items.Add(item);
        cb.EndUpdate();

        if (keep.HasValue)
            foreach (T item in cb.Items)
                if (item.Id == keep.Value) { cb.SelectedItem = item; break; }
    }

    /// <summary>Выполняет действие; бизнес-ошибки показывает пользователю.</summary>
    public static bool Try(IWin32Window owner, Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (BusinessException ex)
        {
            MessageBox.Show(owner, ex.Message, "Проверьте данные", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    public static void Info(IWin32Window owner, string message) =>
        MessageBox.Show(owner, message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static bool Confirm(IWin32Window owner, string message) =>
        MessageBox.Show(owner, message, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    // ---------------------------------------------------- таблица «подпись — поле»

    public static TableLayoutPanel FormTable()
    {
        var t = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(12)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        return t;
    }

    public static Label AddRow(TableLayoutPanel t, string label, Control c, int height = 0, int width = 340)
    {
        int row = t.RowCount;
        t.RowCount = row + 1;

        var l = new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = height > 0 ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Left,
            Margin = new Padding(3, 7, 12, 7)
        };
        c.Width = width;
        if (height > 0) c.Height = height;
        c.Margin = new Padding(3, 4, 3, 4);

        t.Controls.Add(l, 0, row);
        t.Controls.Add(c, 1, row);
        return l;
    }
}
