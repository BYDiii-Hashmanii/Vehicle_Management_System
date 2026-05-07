using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class ReportsPanel : Panel
{
    private DataGridView _grid = null!;
    private DateTimePicker _dtStart = null!, _dtEnd = null!;
    private ComboBox _cboType = null!;
    private ComboBox _cboCustomer = null!;
    private ComboBox _cboVehicle  = null!;

    public ReportsPanel()
    {
        Dock      = DockStyle.Fill;
        BackColor = Color.FromArgb(240, 243, 248);
        Build();
    }

    private void Build()
    {
        // ── Filter card ───────────────────────────────────────────
        var filterCard = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 70,
            BackColor = Color.White,
            Padding   = new Padding(12, 10, 12, 10)
        };

        int x = 12;

        Add(filterCard, "Report Type:", ref x);
        _cboType = new ComboBox
        {
            Location      = new Point(x, 18),
            Width         = 170,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = UIHelper.FontLabel
        };
        _cboType.Items.AddRange(new object[] { "Service Report", "Customer History", "Vehicle Maintenance" });
        _cboType.SelectedIndex = 0;
        _cboType.SelectedIndexChanged += (_, _) => ToggleFilters();
        filterCard.Controls.Add(_cboType);
        x += 178;

        Add(filterCard, "From:", ref x);
        _dtStart = new DateTimePicker { Location = new Point(x, 16), Width = 120, Format = DateTimePickerFormat.Short, Font = UIHelper.FontLabel };
        _dtStart.Value = new DateTime(DateTime.Today.Year, 1, 1);
        filterCard.Controls.Add(_dtStart);
        x += 128;

        Add(filterCard, "To:", ref x);
        _dtEnd = new DateTimePicker { Location = new Point(x, 16), Width = 120, Format = DateTimePickerFormat.Short, Font = UIHelper.FontLabel, Value = DateTime.Today };
        filterCard.Controls.Add(_dtEnd);
        x += 128;

        Add(filterCard, "Customer:", ref x);
        _cboCustomer = new ComboBox { Location = new Point(x, 16), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        _cboCustomer.Items.Add(new ComboItem(0, "(All)"));
        try
        {
            var dt = DB.GetAllCustomers();
            foreach (System.Data.DataRow r in dt.Rows)
                _cboCustomer.Items.Add(new ComboItem(Convert.ToInt32(r["CustomerID"]), r["FullName"].ToString()!));
        }
        catch { }
        _cboCustomer.SelectedIndex = 0;
        _cboCustomer.SelectedIndexChanged += (_, _) => LoadVehiclesForReport();
        filterCard.Controls.Add(_cboCustomer);
        x += 168;

        Add(filterCard, "Vehicle:", ref x);
        _cboVehicle = new ComboBox { Location = new Point(x, 16), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        _cboVehicle.Items.Add(new ComboItem(0, "(All)"));
        _cboVehicle.SelectedIndex = 0;
        filterCard.Controls.Add(_cboVehicle);
        x += 168;

        var btnRun = UIHelper.MakeButton("▶ Run Report", UIHelper.Primary, Color.White, 130, 34);
        btnRun.Location = new Point(x, 14);
        btnRun.Click   += (_, _) => RunReport();
        filterCard.Controls.Add(btnRun);

        // ── Grid ─────────────────────────────────────────────────
        var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0) };
        _grid = new DataGridView { Dock = DockStyle.Fill };
        UIHelper.StyleGrid(_grid);
        gridCard.Controls.Add(_grid);

        Controls.Add(gridCard);
        Controls.Add(filterCard);

        ToggleFilters();
    }

    private static void Add(Panel p, string text, ref int x)
    {
        var lbl = new Label
        {
            Text     = text,
            AutoSize = true,
            Font     = UIHelper.FontLabel,
            Location = new Point(x, 22),
            ForeColor = Color.Gray
        };
        p.Controls.Add(lbl);
        x += lbl.PreferredWidth + 4;
    }

    private void ToggleFilters()
    {
        bool isService  = _cboType.SelectedIndex == 0;
        bool isCust     = _cboType.SelectedIndex == 1;
        bool isVeh      = _cboType.SelectedIndex == 2;

        _dtStart.Enabled     = isService;
        _dtEnd.Enabled       = isService;
        _cboCustomer.Enabled = isCust;
        _cboVehicle.Enabled  = isVeh;
    }

    private void LoadVehiclesForReport()
    {
        _cboVehicle.Items.Clear();
        _cboVehicle.Items.Add(new ComboItem(0, "(All)"));
        _cboVehicle.SelectedIndex = 0;
        if (_cboCustomer.SelectedItem is not ComboItem c || c.Id == 0) return;
        try
        {
            var dt = DB.GetVehiclesByCustomer(c.Id);
            foreach (System.Data.DataRow r in dt.Rows)
                _cboVehicle.Items.Add(new ComboItem(Convert.ToInt32(r["VehicleID"]), r["Label"].ToString()!));
        }
        catch { }
    }

    private void RunReport()
    {
        try
        {
            System.Data.DataTable? dt = _cboType.SelectedIndex switch
            {
                0 => DB.GetServiceReport(_dtStart.Value, _dtEnd.Value),
                1 when _cboCustomer.SelectedItem is ComboItem c && c.Id > 0
                     => DB.GetCustomerHistory(c.Id),
                2 when _cboVehicle.SelectedItem is ComboItem v && v.Id > 0
                     => DB.GetVehicleMaintenanceRecords(v.Id),
                1    => ShowMsg("Select a customer for Customer History."),
                2    => ShowMsg("Select a vehicle for Vehicle Maintenance report."),
                _    => null
            };
            if (dt != null) _grid.DataSource = dt;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static System.Data.DataTable? ShowMsg(string msg)
    {
        MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }
}
