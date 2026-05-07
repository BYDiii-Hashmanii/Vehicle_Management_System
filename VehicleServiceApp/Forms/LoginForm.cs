using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class LoginForm : Form
{
    private TextBox txtUser = null!, txtPass = null!;
    private Label lblError = null!;

    public LoginForm()
    {
        InitUI();
    }

    private void InitUI()
    {
        Text            = "Vehicle Service Management — Login";
        Size            = new Size(420, 520);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = UIHelper.Sidebar;

        // ── Logo area ────────────────────────────────────────────
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = UIHelper.Sidebar };

        var lblIcon = new Label
        {
            Text      = "🔧",
            Font      = new Font("Segoe UI Emoji", 36),
            ForeColor = UIHelper.Primary,
            AutoSize  = false,
            Size      = new Size(380, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Location  = new Point(0, 20)
        };

        var lblTitle = new Label
        {
            Text      = "Vehicle Service MS",
            Font      = UIHelper.FontTitle,
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(380, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location  = new Point(0, 90)
        };

        pnlTop.Controls.AddRange(new Control[] { lblIcon, lblTitle });

        // ── Card ─────────────────────────────────────────────────
        var card = new Panel
        {
            Location  = new Point(30, 155),
            Size      = new Size(345, 300),
            BackColor = Color.White,
            Padding   = new Padding(30)
        };

        int y = 30;
        AddField(card, "Username", ref y, out txtUser, false);
        AddField(card, "Password", ref y, out txtPass, true);

        lblError = new Label
        {
            Location  = new Point(20, y + 5),
            Size      = new Size(300, 22),
            ForeColor = UIHelper.Danger,
            Font      = UIHelper.FontLabel,
            Text      = ""
        };
        card.Controls.Add(lblError);

        y += 35;
        var btnLogin = UIHelper.MakeButton("Login", UIHelper.Primary, Color.White, 305, 40);
        btnLogin.Location = new Point(20, y);
        btnLogin.Font     = new Font("Segoe UI", 11, FontStyle.Bold);
        btnLogin.Click   += BtnLogin_Click;
        card.Controls.Add(btnLogin);

        var lblHint = new Label
        {
            Text      = "Default: admin / admin123",
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            AutoSize  = true,
            Location  = new Point(95, y + 48)
        };
        card.Controls.Add(lblHint);

        Controls.AddRange(new Control[] { pnlTop, card });

        // Enter key triggers login
        AcceptButton = btnLogin;
    }

    private void AddField(Panel parent, string label, ref int y, out TextBox txt, bool pwd)
    {
        var lbl = new Label
        {
            Text     = label,
            Location = new Point(20, y),
            AutoSize = true,
            Font     = UIHelper.FontBtn,
            ForeColor = Color.FromArgb(60, 60, 60)
        };
        y += 22;
        txt = new TextBox
        {
            Location      = new Point(20, y),
            Size          = new Size(305, 28),
            Font          = UIHelper.FontLabel,
            BorderStyle   = BorderStyle.FixedSingle,
            UseSystemPasswordChar = pwd
        };
        y += 44;
        parent.Controls.AddRange(new Control[] { lbl, txt });
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";
        var user = txtUser.Text.Trim();
        var pass = txtPass.Text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            lblError.Text = "Please enter username and password."; return;
        }

        try
        {
            var hash = UIHelper.SimpleHash(pass);
            var dt   = DB.ValidateLogin(user, hash);
            if (dt.Rows.Count == 0)
            {
                lblError.Text = "Invalid username or password."; return;
            }

            var row = dt.Rows[0];
            Session.UserID   = Convert.ToInt32(row["UserID"]);
            Session.FullName = row["FullName"].ToString()!;
            Session.Role     = row["Role"].ToString()!;

            Hide();
            new MainForm().ShowDialog();
            Show();
            txtUser.Clear();
            txtPass.Clear();
        }
        catch (Exception ex)
        {
            lblError.Text = "DB Error: " + ex.Message;
        }
    }
}
