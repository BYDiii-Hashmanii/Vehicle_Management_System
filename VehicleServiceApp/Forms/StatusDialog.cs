namespace VehicleServiceApp.Forms;

/// <summary>
/// Small popup to pick a new status from a fixed list and fire a callback.
/// </summary>
public class StatusDialog : Form
{
    public StatusDialog(string current, string[] options, Action<string> onConfirm)
    {
        Text            = "Update Status";
        Size            = new Size(300, 180);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;
        Padding         = new Padding(16);

        var lbl = UIHelper.MakeLabel("Select new status:");
        lbl.Location = new Point(16, 16);

        var cbo = new ComboBox
        {
            Location      = new Point(16, 38),
            Width         = 245,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = UIHelper.FontLabel
        };
        cbo.Items.AddRange(options);
        cbo.Text = current;

        var btnOk     = UIHelper.MakeButton("Update",  UIHelper.Primary, Color.White, 110, 32);
        var btnCancel = UIHelper.MakeButton("Cancel",  Color.Gray,       Color.White, 110, 32);
        btnOk.Location     = new Point(16,  96);
        btnCancel.Location = new Point(134, 96);

        btnOk.Click += (_, _) =>
        {
            if (cbo.SelectedItem == null) { MessageBox.Show("Pick a status."); return; }
            try { onConfirm(cbo.SelectedItem.ToString()!); Close(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        };
        btnCancel.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { lbl, cbo, btnOk, btnCancel });
        AcceptButton = btnOk;
    }
}
