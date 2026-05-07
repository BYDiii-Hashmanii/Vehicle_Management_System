using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class SchedulesPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllSchedules(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new ScheduleDialog().ShowDialog();

    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("ScheduleID");
        if (id < 0) return;
        var row = Grid.SelectedRows[0];
        new ScheduleDialog(id,
            row.Cells["ServiceType"].Value?.ToString() ?? "",
            Convert.ToDateTime(row.Cells["ScheduledDate"].Value),
            row.Cells["Status"].Value?.ToString() ?? "Scheduled",
            row.Cells["Notes"].Value?.ToString() ?? ""
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        int id = GetSelectedId("ScheduleID");
        if (id < 0) return;
        if (MessageBox.Show("Cancel this schedule?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            DB.UpdateScheduleStatus(id, "Cancelled");
            LoadData();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    protected override void AddCustomButtons(Panel toolbar)
    {
        var btnStatus = UIHelper.MakeButton("⚡ Update Status", UIHelper.Success, Color.White, 140, 34);
        btnStatus.Location = new Point(490, 9);
        btnStatus.Click   += (_, _) =>
        {
            if (!HasSelection()) return;
            int id  = GetSelectedId("ScheduleID");
            var cur = Grid.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "Scheduled";
            new StatusDialog(cur, new[] { "Scheduled", "In Progress", "Completed", "Cancelled" },
                status => { DB.UpdateScheduleStatus(id, status); LoadData(); }
            ).ShowDialog();
        };
        toolbar.Controls.Add(btnStatus);
    }
}

// ── Schedule Add/Edit Dialog ──────────────────────────────────────────────────
public class ScheduleDialog : Form
{
    private readonly int _id;
    private ComboBox _cboCustomer = null!, _cboVehicle = null!, _cboStatus = null!;
    private DateTimePicker _date = null!;
    private TextBox _type = null!, _notes = null!;

    public ScheduleDialog(int id = 0, string serviceType = "", DateTime date = default,
                          string status = "Scheduled", string notes = "")
    {
        _id   = id;
        Text  = id == 0 ? "Schedule Service" : "Edit Schedule";
        Size  = new Size(430, 400);
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Customer
        layout.Controls.Add(UIHelper.MakeLabel("Customer*"));
        _cboCustomer = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        try
        {
            var dt = DB.GetAllCustomers();
            foreach (System.Data.DataRow r in dt.Rows)
                _cboCustomer.Items.Add(new ComboItem(Convert.ToInt32(r["CustomerID"]), r["FullName"].ToString()!));
        }
        catch { }
        _cboCustomer.SelectedIndexChanged += (_, _) => LoadVehicles();
        layout.Controls.Add(_cboCustomer);

        // Vehicle
        layout.Controls.Add(UIHelper.MakeLabel("Vehicle*"));
        _cboVehicle = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        layout.Controls.Add(_cboVehicle);

        // Date
        layout.Controls.Add(UIHelper.MakeLabel("Date*"));
        _date = new DateTimePicker
        {
            Width  = 250,
            Format = DateTimePickerFormat.Short,
            Value  = date == default ? DateTime.Today : date,
            Font   = UIHelper.FontLabel
        };
        layout.Controls.Add(_date);

        // Service type
        layout.Controls.Add(UIHelper.MakeLabel("Service Type*"));
        _type = UIHelper.MakeTextBox(250);
        _type.Text = serviceType;
        layout.Controls.Add(_type);

        // Status (edit only)
        layout.Controls.Add(UIHelper.MakeLabel("Status"));
        _cboStatus = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        _cboStatus.Items.AddRange(new object[] { "Scheduled", "In Progress", "Completed", "Cancelled" });
        _cboStatus.Text    = status;
        _cboStatus.Enabled = id != 0;
        layout.Controls.Add(_cboStatus);

        // Notes
        layout.Controls.Add(UIHelper.MakeLabel("Notes"));
        _notes = UIHelper.MakeTextBox(250, true, 3);
        _notes.Text = notes;
        layout.Controls.Add(_notes);

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

    private void LoadVehicles()
    {
        _cboVehicle.Items.Clear();
        if (_cboCustomer.SelectedItem is not ComboItem c) return;
        try
        {
            var dt = DB.GetVehiclesByCustomer(c.Id);
            foreach (System.Data.DataRow r in dt.Rows)
                _cboVehicle.Items.Add(new ComboItem(Convert.ToInt32(r["VehicleID"]), r["Label"].ToString()!));
            if (_cboVehicle.Items.Count > 0) _cboVehicle.SelectedIndex = 0;
        }
        catch { }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_id == 0 && (_cboCustomer.SelectedItem == null || _cboVehicle.SelectedItem == null))
        {
            MessageBox.Show("Select customer and vehicle.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        if (string.IsNullOrWhiteSpace(_type.Text))
        {
            MessageBox.Show("Service Type is required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        try
        {
            if (_id == 0)
            {
                var cust = (ComboItem)_cboCustomer.SelectedItem!;
                var veh  = (ComboItem)_cboVehicle.SelectedItem!;
                DB.AddSchedule(veh.Id, cust.Id, _date.Value, _type.Text.Trim(),
                               _notes.Text.Trim() == "" ? null : _notes.Text.Trim(),
                               Session.UserID);
            }
            else
            {
                DB.UpdateScheduleStatus(_id, _cboStatus.Text);
            }
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
