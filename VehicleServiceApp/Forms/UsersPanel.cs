using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class UsersPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllUsers(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new UserDialog().ShowDialog();
    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("UserID");
        if (id < 0) return;
        var row = Grid.SelectedRows[0];
        new UserDialog(id,
            row.Cells["FullName"].Value?.ToString() ?? "",
            row.Cells["Role"].Value?.ToString() ?? "Staff",
            row.Cells["Email"].Value?.ToString() ?? "",
            row.Cells["Phone"].Value?.ToString() ?? "",
            Convert.ToBoolean(row.Cells["IsActive"].Value)
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        int id = GetSelectedId("UserID");
        if (id == Session.UserID)
        {
            MessageBox.Show("You cannot deactivate your own account.", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        if (MessageBox.Show("Deactivate this user?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            DB.DeleteUser(id);
            LoadData();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

// ── User Add/Edit Dialog ──────────────────────────────────────────────────────
public class UserDialog : Form
{
    private readonly int _id;
    private TextBox  _username = null!, _pass = null!, _fullName = null!, _email = null!, _phone = null!;
    private ComboBox _role     = null!;
    private CheckBox _active   = null!;

    public UserDialog(int id = 0, string fullName = "", string role = "Staff",
                      string email = "", string phone = "", bool isActive = true)
    {
        _id   = id;
        Text  = id == 0 ? "Add User" : "Edit User";
        Size  = new Size(390, 420);
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

        if (id == 0)
        {
            AddRow(layout, "Username*",   out _username, "");
            AddRow(layout, "Password*",   out _pass,     "");
            _pass.UseSystemPasswordChar = true;
        }
        else
        {
            // Hidden stubs (needed for compile, not shown)
            _username = new TextBox();
            _pass     = new TextBox();
        }

        AddRow(layout, "Full Name*", out _fullName, fullName);
        AddRow(layout, "Email",      out _email,    email);
        AddRow(layout, "Phone",      out _phone,    phone);

        layout.Controls.Add(UIHelper.MakeLabel("Role*"));
        _role = new ComboBox
        {
            Width         = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = UIHelper.FontLabel
        };
        _role.Items.AddRange(new object[] { "Admin", "Staff" });
        _role.Text = role;
        layout.Controls.Add(_role);

        if (id != 0)
        {
            layout.Controls.Add(UIHelper.MakeLabel("Active"));
            _active = new CheckBox { Checked = isActive, Font = UIHelper.FontLabel, AutoSize = true };
            layout.Controls.Add(_active);
        }
        else
        {
            _active = new CheckBox { Checked = true };
        }

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

    private static void AddRow(TableLayoutPanel tbl, string lbl, out TextBox txt, string val)
    {
        tbl.Controls.Add(UIHelper.MakeLabel(lbl));
        txt = UIHelper.MakeTextBox(220);
        txt.Text = val;
        tbl.Controls.Add(txt);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_fullName.Text) || _role.SelectedItem == null)
        {
            MessageBox.Show("Full Name and Role are required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        try
        {
            if (_id == 0)
            {
                if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_pass.Text))
                {
                    MessageBox.Show("Username and Password are required.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
                DB.AddUser(_username.Text.Trim(), UIHelper.SimpleHash(_pass.Text),
                           _fullName.Text.Trim(), _role.Text,
                           NullIfEmpty(_email.Text), NullIfEmpty(_phone.Text));
            }
            else
            {
                DB.UpdateUser(_id, _fullName.Text.Trim(), _role.Text,
                              NullIfEmpty(_email.Text), NullIfEmpty(_phone.Text),
                              _active.Checked);
            }
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
