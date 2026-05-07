using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class CustomersPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllCustomers(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new CustomerDialog().ShowDialog();
    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("CustomerID");
        if (id < 0) return;
        var row = Grid.SelectedRows[0];
        new CustomerDialog(
            id,
            row.Cells["FullName"].Value?.ToString() ?? "",
            row.Cells["Email"].Value?.ToString() ?? "",
            row.Cells["Phone"].Value?.ToString() ?? "",
            row.Cells["Address"].Value?.ToString() ?? ""
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        int id = GetSelectedId("CustomerID");
        if (id < 0) return;
        if (MessageBox.Show("Delete this customer?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            DB.DeleteCustomer(id);
            LoadData();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

// ── Customer Add/Edit Dialog ──────────────────────────────────────────────────
public class CustomerDialog : Form
{
    private readonly int _id;
    private TextBox _name = null!, _email = null!, _phone = null!, _addr = null!;

    public CustomerDialog(int id = 0, string name = "", string email = "",
                          string phone = "", string addr = "")
    {
        _id   = id;
        Text  = id == 0 ? "Add Customer" : "Edit Customer";
        Size  = new Size(380, 310);
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Full Name*",  out _name,  name);
        AddRow(layout, "Email",       out _email, email);
        AddRow(layout, "Phone*",      out _phone, phone);
        AddRow(layout, "Address",     out _addr,  addr);

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
        txt = UIHelper.MakeTextBox(190);
        txt.Text = val;
        tbl.Controls.Add(txt);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_phone.Text))
        {
            MessageBox.Show("Name and Phone are required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        try
        {
            if (_id == 0)
                DB.AddCustomer(_name.Text.Trim(), NullIfEmpty(_email.Text),
                               _phone.Text.Trim(), NullIfEmpty(_addr.Text));
            else
                DB.UpdateCustomer(_id, _name.Text.Trim(), NullIfEmpty(_email.Text),
                                  _phone.Text.Trim(), NullIfEmpty(_addr.Text));
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
