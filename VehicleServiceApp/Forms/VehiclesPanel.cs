using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class VehiclesPanel : CrudPanel
{
    protected override void LoadData()
    {
        try { Grid.DataSource = DB.GetAllVehicles(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    protected override void OpenAddDialog()  => new VehicleDialog().ShowDialog();
    protected override void OpenEditDialog()
    {
        int id = GetSelectedId("VehicleID");
        if (id < 0) return;
        var row = Grid.SelectedRows[0];
        new VehicleDialog(id,
            row.Cells["Make"].Value?.ToString() ?? "",
            row.Cells["Model"].Value?.ToString() ?? "",
            Convert.ToInt32(row.Cells["Year"].Value),
            row.Cells["LicensePlate"].Value?.ToString() ?? "",
            Convert.ToInt32(row.Cells["Mileage"].Value)
        ).ShowDialog();
    }

    protected override void DeleteSelected()
    {
        int id = GetSelectedId("VehicleID");
        if (id < 0) return;
        if (MessageBox.Show("Delete this vehicle?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            DB.DeleteVehicle(id);
            LoadData();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

// ── Vehicle Add/Edit Dialog ───────────────────────────────────────────────────
public class VehicleDialog : Form
{
    private readonly int _id;
    private ComboBox _cboCustomer = null!;
    private TextBox _make = null!, _model = null!, _plate = null!;
    private NumericUpDown _year = null!, _mileage = null!;

    public VehicleDialog(int id = 0, string make = "", string model = "",
                         int year = 2020, string plate = "", int mileage = 0)
    {
        _id   = id;
        Text  = id == 0 ? "Add Vehicle" : "Edit Vehicle";
        Size  = new Size(400, 380);
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

        // Customer dropdown
        layout.Controls.Add(UIHelper.MakeLabel("Customer*"));
        _cboCustomer = new ComboBox
        {
            Width         = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = UIHelper.FontLabel
        };
        try
        {
            var dt = DB.GetAllCustomers();
            foreach (System.Data.DataRow r in dt.Rows)
                _cboCustomer.Items.Add(new ComboItem(Convert.ToInt32(r["CustomerID"]), r["FullName"].ToString()!));
        }
        catch { }
        layout.Controls.Add(_cboCustomer);

        AddRow(layout, "Make*",         out _make,  make);
        AddRow(layout, "Model*",        out _model, model);

        layout.Controls.Add(UIHelper.MakeLabel("Year*"));
        _year = new NumericUpDown { Minimum = 1900, Maximum = 2100, Value = year, Width = 220, Font = UIHelper.FontLabel };
        layout.Controls.Add(_year);

        AddRow(layout, "License Plate*", out _plate, plate);

        layout.Controls.Add(UIHelper.MakeLabel("Mileage"));
        _mileage = new NumericUpDown { Minimum = 0, Maximum = 9999999, Value = mileage, Width = 220, Font = UIHelper.FontLabel };
        layout.Controls.Add(_mileage);

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
        if (_id == 0 && _cboCustomer.SelectedItem == null)
        {
            MessageBox.Show("Select a customer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        if (string.IsNullOrWhiteSpace(_make.Text) || string.IsNullOrWhiteSpace(_model.Text) ||
            string.IsNullOrWhiteSpace(_plate.Text))
        {
            MessageBox.Show("Make, Model and License Plate are required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        try
        {
            if (_id == 0)
            {
                var cust = (ComboItem)_cboCustomer.SelectedItem!;
                DB.AddVehicle(cust.Id, _make.Text.Trim(), _model.Text.Trim(),
                              (int)_year.Value, _plate.Text.Trim(), (int)_mileage.Value);
            }
            else
            {
                DB.UpdateVehicle(_id, _make.Text.Trim(), _model.Text.Trim(),
                                 (int)_year.Value, _plate.Text.Trim(), (int)_mileage.Value);
            }
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

// Shared helper for ComboBox items (ID + display text)
public record ComboItem(int Id, string Label) { public override string ToString() => Label; }
