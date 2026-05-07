namespace VehicleServiceApp.Forms;

/// <summary>
/// Base class for all CRUD panels.
/// Provides a toolbar (Add / Edit / Delete / Refresh buttons) and a DataGridView.
/// Subclasses override LoadData(), OpenAddDialog(), OpenEditDialog(), DeleteSelected().
/// </summary>
public abstract class CrudPanel : Panel
{
    protected DataGridView Grid = null!;
    protected Button BtnAdd    = null!;
    protected Button BtnEdit   = null!;
    protected Button BtnDelete = null!;
    protected Button BtnRefresh = null!;
    protected TextBox TxtSearch = null!;

    protected CrudPanel()
    {
        BackColor = Color.FromArgb(240, 243, 248);
        Dock      = DockStyle.Fill;
        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        // ── Toolbar ───────────────────────────────────────────────
        var toolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = Color.White,
            Padding   = new Padding(10, 8, 10, 8)
        };

        BtnAdd = UIHelper.MakeButton("➕ Add",    UIHelper.Primary, Color.White);
        BtnAdd.Location = new Point(10, 9);
        BtnAdd.Click   += (_, _) => { OpenAddDialog(); LoadData(); };

        BtnEdit = UIHelper.MakeButton("✏️ Edit", UIHelper.Warning, Color.White);
        BtnEdit.Location = new Point(128, 9);
        BtnEdit.Click   += (_, _) => { if (HasSelection()) { OpenEditDialog(); LoadData(); } };

        BtnDelete = UIHelper.MakeButton("🗑 Delete", UIHelper.Danger, Color.White);
        BtnDelete.Location = new Point(246, 9);
        BtnDelete.Click   += (_, _) => { if (HasSelection()) DeleteSelected(); };

        BtnRefresh = UIHelper.MakeButton("🔄 Refresh", Color.FromArgb(108, 117, 125), Color.White);
        BtnRefresh.Location = new Point(364, 9);
        BtnRefresh.Click   += (_, _) => LoadData();

        var lblSearch = new Label
        {
            Text      = "Search:",
            AutoSize  = true,
            Font      = UIHelper.FontLabel,
            Location  = new Point(500, 14),
            ForeColor = Color.Gray
        };
        TxtSearch = new TextBox
        {
            Location    = new Point(552, 11),
            Size        = new Size(180, 26),
            Font        = UIHelper.FontLabel,
            BorderStyle = BorderStyle.FixedSingle
        };
        TxtSearch.TextChanged += (_, _) => FilterGrid(TxtSearch.Text);

        toolbar.Controls.AddRange(new Control[]
            { BtnAdd, BtnEdit, BtnDelete, BtnRefresh, lblSearch, TxtSearch });

        // ── Grid ─────────────────────────────────────────────────
        Grid = new DataGridView { Dock = DockStyle.Fill };
        UIHelper.StyleGrid(Grid);

        // ── Card wrapper ─────────────────────────────────────────
        var card = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.White,
            Margin    = new Padding(0, 10, 0, 0)
        };
        card.Controls.Add(Grid);
        card.Controls.Add(toolbar);

        Controls.Add(card);

        AddCustomButtons(toolbar);
    }

    /// <summary>Override to add extra toolbar buttons after the defaults.</summary>
    protected virtual void AddCustomButtons(Panel toolbar) { }

    protected abstract void LoadData();
    protected abstract void OpenAddDialog();
    protected abstract void OpenEditDialog();
    protected abstract void DeleteSelected();

    protected bool HasSelection()
    {
        if (Grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a row first.", "No Selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        return true;
    }

    protected int GetSelectedId(string columnName)
    {
        if (Grid.SelectedRows.Count == 0) return -1;
        return Convert.ToInt32(Grid.SelectedRows[0].Cells[columnName].Value);
    }

    private void FilterGrid(string text)
    {
        if (Grid.DataSource is not System.Data.DataTable dt) return;
        if (string.IsNullOrWhiteSpace(text))
        {
            dt.DefaultView.RowFilter = "";
            return;
        }
        // Build filter across all string columns
        var filters = new List<string>();
        foreach (System.Data.DataColumn col in dt.Columns)
        {
            if (col.DataType == typeof(string))
                filters.Add($"[{col.ColumnName}] LIKE '%{text.Replace("'", "''")}%'");
        }
        dt.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" OR ", filters) : "";
    }
}
