using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class ServiceDetailsPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllServiceDetails(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new ServiceDetailDialog().ShowDialog();
    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("ServiceID");
        if (id < 0) return;
        var row = Grid.SelectedRows[0];
        new ServiceDetailDialog(id,
            row.Cells["Status"].Value?.ToString() ?? "Pending",
            Convert.ToDecimal(row.Cells["ServiceCost"].Value),
            row.Cells["PartsUsed"].Value?.ToString() ?? ""
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        MessageBox.Show("Service records cannot be deleted — only updated.", "Info",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

// ── Service Detail Add Dialog ─────────────────────────────────────────────────
public class ServiceDetailDialog : Form
{
    private readonly int _id;
    private ComboBox _cboSchedule = null!, _cboTech = null!, _cboStatus = null!;
    private TextBox  _desc = null!, _parts = null!;
    private NumericUpDown _cost = null!;

    // Add mode
    public ServiceDetailDialog()
    {
        _id   = 0;
        Text  = "Add Service Detail";
        Init("Pending", 0, "");
    }

    // Edit mode
    public ServiceDetailDialog(int id, string status, decimal cost, string parts)
    {
        _id   = id;
        Text  = "Update Service";
        Init(status, cost, parts);
    }

    private void Init(string status, decimal cost, string parts)
    {
        Size            = new Size(430, 440);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            Padding     = new Padding(16),
            AutoSize    = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        bool isAdd = _id == 0;

        if (isAdd)
        {
            // Schedule dropdown
            layout.Controls.Add(UIHelper.MakeLabel("Schedule*"));
            _cboSchedule = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
            try
            {
                var dt = DB.ExecuteQuery(
                    "SELECT ss.ScheduleID, c.FullName+' - '+ss.ServiceType+' ('+CAST(ss.ScheduledDate AS VARCHAR)+')' AS Lbl " +
                    "FROM ServiceSchedule ss JOIN Customers c ON ss.CustomerID=c.CustomerID WHERE ss.Status='In Progress' OR ss.Status='Scheduled'");
                foreach (System.Data.DataRow r in dt.Rows)
                    _cboSchedule.Items.Add(new ComboItem(Convert.ToInt32(r["ScheduleID"]), r["Lbl"].ToString()!));
            }
            catch { }
            layout.Controls.Add(_cboSchedule);

            // Technician
            layout.Controls.Add(UIHelper.MakeLabel("Technician"));
            _cboTech = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
            _cboTech.Items.Add(new ComboItem(0, "(None)"));
            try
            {
                var dt = DB.GetAllUsers_Staff();
                foreach (System.Data.DataRow r in dt.Rows)
                    _cboTech.Items.Add(new ComboItem(Convert.ToInt32(r["UserID"]), r["FullName"].ToString()!));
            }
            catch { }
            _cboTech.SelectedIndex = 0;
            layout.Controls.Add(_cboTech);

            // Description
            layout.Controls.Add(UIHelper.MakeLabel("Description*"));
            _desc = UIHelper.MakeTextBox(240, true, 3);
            layout.Controls.Add(_desc);
        }

        // Status
        layout.Controls.Add(UIHelper.MakeLabel("Status*"));
        _cboStatus = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        _cboStatus.Items.AddRange(new object[] { "Pending", "In Progress", "Completed" });
        _cboStatus.Text = status;
        layout.Controls.Add(_cboStatus);

        // Cost
        layout.Controls.Add(UIHelper.MakeLabel("Service Cost"));
        _cost = new NumericUpDown
        {
            Width         = 240,
            Minimum       = 0,
            Maximum       = 9999999,
            DecimalPlaces = 2,
            Value         = cost,
            Font          = UIHelper.FontLabel
        };
        layout.Controls.Add(_cost);

        // Parts
        layout.Controls.Add(UIHelper.MakeLabel("Parts Used"));
        _parts = UIHelper.MakeTextBox(240, true, 2);
        _parts.Text = parts;
        layout.Controls.Add(_parts);

        var btnSave   = UIHelper.MakeButton("Save",   UIHelper.Primary, Color.White, 100, 32);
        var btnCancel = UIHelper.MakeButton("Cancel", Color.Gray,       Color.White, 100, 32);
        btnSave.Click   += BtnSave_Click;
        btnCancel.Click += (_, _) => Close();

        var btnPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock          = DockStyle.Bottom,
            Height        = 44,
            Padding       = new Padding(10, 5, 10, 5)
        };
        btnPanel.Controls.AddRange(new Control[] { btnCancel, btnSave });
        Controls.AddRange(new Control[] { layout, btnPanel });
        AcceptButton = btnSave;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_id == 0)
            {
                if (_cboSchedule.SelectedItem is not ComboItem sched)
                {
                    MessageBox.Show("Select a schedule.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
                if (string.IsNullOrWhiteSpace(_desc.Text))
                {
                    MessageBox.Show("Description is required.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }

                // Get vehicleID from schedule
                var dt = DB.ExecuteQuery(
                    "SELECT VehicleID FROM ServiceSchedule WHERE ScheduleID=@id",
                    new Microsoft.Data.SqlClient.SqlParameter("@id", sched.Id));
                int vehicleId = Convert.ToInt32(dt.Rows[0]["VehicleID"]);

                int? techId = null;
                if (_cboTech.SelectedItem is ComboItem t && t.Id > 0) techId = t.Id;

                DB.AddServiceDetail(sched.Id, vehicleId, techId, _desc.Text.Trim(),
                                   _cost.Value,
                                   _parts.Text.Trim() == "" ? null : _parts.Text.Trim());
            }
            else
            {
                DB.UpdateServiceStatus(_id, _cboStatus.Text, _cost.Value,
                                       _parts.Text.Trim() == "" ? null : _parts.Text.Trim());
            }
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
