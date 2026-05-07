using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class ComplaintsPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllComplaints(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new ComplaintDialog().ShowDialog();
    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("ComplaintID");
        if (id < 0) return;
        new AssignResolveDialog(id,
            Grid.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "Open"
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        MessageBox.Show("Complaints cannot be deleted — use Resolve/Close instead.", "Info",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void AddCustomButtons(Panel toolbar)
    {
        var btnAssign  = UIHelper.MakeButton("👤 Assign",  UIHelper.Primary, Color.White, 110, 34);
        var btnResolve = UIHelper.MakeButton("✅ Resolve", UIHelper.Success, Color.White, 110, 34);

        btnAssign.Location  = new Point(490, 9);
        btnResolve.Location = new Point(608, 9);

        btnAssign.Click += (_, _) =>
        {
            if (!HasSelection()) return;
            int id = GetSelectedId("ComplaintID");
            new AssignDialog(id, () => LoadData()).ShowDialog();
        };

        btnResolve.Click += (_, _) =>
        {
            if (!HasSelection()) return;
            int id = GetSelectedId("ComplaintID");
            new ResolveDialog(id, () => LoadData()).ShowDialog();
        };

        toolbar.Controls.AddRange(new Control[] { btnAssign, btnResolve });
    }
}

// ── Add Complaint ─────────────────────────────────────────────────────────────
public class ComplaintDialog : Form
{
    private ComboBox _cboCustomer = null!, _cboVehicle = null!;
    private TextBox  _subject = null!, _desc = null!;

    public ComplaintDialog()
    {
        Text            = "New Complaint";
        Size            = new Size(420, 380);
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

        layout.Controls.Add(UIHelper.MakeLabel("Vehicle"));
        _cboVehicle = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontLabel };
        layout.Controls.Add(_cboVehicle);

        layout.Controls.Add(UIHelper.MakeLabel("Subject*"));
        _subject = UIHelper.MakeTextBox(250);
        layout.Controls.Add(_subject);

        layout.Controls.Add(UIHelper.MakeLabel("Description*"));
        _desc = UIHelper.MakeTextBox(250, true, 4);
        layout.Controls.Add(_desc);

        var btnSave   = UIHelper.MakeButton("Submit",  UIHelper.Danger,  Color.White, 100, 32);
        var btnCancel = UIHelper.MakeButton("Cancel",  Color.Gray,       Color.White, 100, 32);
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
        _cboVehicle.Items.Add(new ComboItem(0, "(No vehicle)"));
        _cboVehicle.SelectedIndex = 0;
        if (_cboCustomer.SelectedItem is not ComboItem c) return;
        try
        {
            var dt = DB.GetVehiclesByCustomer(c.Id);
            foreach (System.Data.DataRow r in dt.Rows)
                _cboVehicle.Items.Add(new ComboItem(Convert.ToInt32(r["VehicleID"]), r["Label"].ToString()!));
        }
        catch { }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cboCustomer.SelectedItem == null || string.IsNullOrWhiteSpace(_subject.Text) ||
            string.IsNullOrWhiteSpace(_desc.Text))
        {
            MessageBox.Show("Customer, Subject and Description are required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        try
        {
            var cust = (ComboItem)_cboCustomer.SelectedItem!;
            int? vid = null;
            if (_cboVehicle.SelectedItem is ComboItem v && v.Id > 0) vid = v.Id;
            DB.AddComplaint(cust.Id, vid, _subject.Text.Trim(), _desc.Text.Trim());
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

// ── Assign Dialog ─────────────────────────────────────────────────────────────
public class AssignDialog : Form
{
    public AssignDialog(int complaintId, Action onDone)
    {
        Text            = "Assign Complaint";
        Size            = new Size(340, 180);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;
        Padding         = new Padding(16);

        var lbl = UIHelper.MakeLabel("Assign to Staff:");
        lbl.Location = new Point(16, 20);

        var cbo = new ComboBox
        {
            Location      = new Point(16, 42),
            Width         = 280,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = UIHelper.FontLabel
        };
        try
        {
            var dt = DB.GetAllUsersActive();
            foreach (System.Data.DataRow r in dt.Rows)
                cbo.Items.Add(new ComboItem(Convert.ToInt32(r["UserID"]), r["FullName"].ToString()!));
        }
        catch { }

        var btnOk     = UIHelper.MakeButton("Assign",  UIHelper.Primary, Color.White, 100, 32);
        var btnCancel = UIHelper.MakeButton("Cancel",  Color.Gray,       Color.White, 100, 32);
        btnOk.Location     = new Point(16,  96);
        btnCancel.Location = new Point(124, 96);

        btnOk.Click += (_, _) =>
        {
            if (cbo.SelectedItem is not ComboItem u) { MessageBox.Show("Select a user."); return; }
            try { DB.AssignComplaint(complaintId, u.Id); onDone(); Close(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        };
        btnCancel.Click += (_, _) => Close();
        Controls.AddRange(new Control[] { lbl, cbo, btnOk, btnCancel });
    }
}

// ── Resolve Dialog ────────────────────────────────────────────────────────────
public class ResolveDialog : Form
{
    public ResolveDialog(int complaintId, Action onDone)
    {
        Text            = "Resolve Complaint";
        Size            = new Size(380, 240);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var lbl = UIHelper.MakeLabel("Resolution Notes:");
        lbl.Location = new Point(16, 16);

        var txt = new TextBox
        {
            Location    = new Point(16, 36),
            Size        = new Size(330, 100),
            Multiline   = true,
            ScrollBars  = ScrollBars.Vertical,
            Font        = UIHelper.FontLabel,
            BorderStyle = BorderStyle.FixedSingle
        };

        var btnOk     = UIHelper.MakeButton("Resolve",  UIHelper.Success, Color.White, 100, 32);
        var btnCancel = UIHelper.MakeButton("Cancel",   Color.Gray,       Color.White, 100, 32);
        btnOk.Location     = new Point(16,  148);
        btnCancel.Location = new Point(124, 148);

        btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                MessageBox.Show("Please enter resolution notes.", "Validation"); return;
            }
            try { DB.ResolveComplaint(complaintId, txt.Text.Trim()); onDone(); Close(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        };
        btnCancel.Click += (_, _) => Close();
        Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
    }
}

// Dummy – just reuses AssignResolveDialog label (not used directly)
public class AssignResolveDialog : Form
{
    public AssignResolveDialog(int id, string status)
    {
        MessageBox.Show($"Complaint #{id} — Status: {status}\nUse Assign or Resolve buttons.", "Info");
        Close();
    }
}
