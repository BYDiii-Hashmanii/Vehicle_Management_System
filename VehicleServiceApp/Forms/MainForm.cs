namespace VehicleServiceApp.Forms;

public class MainForm : Form
{
    private Panel  pnlSidebar = null!;
    private Panel  pnlContent = null!;
    private Label  lblPageTitle = null!;
    private Button? _activeBtn;

    private static readonly string[] NavItems =
    {
        "🏠  Dashboard",
        "👥  Customers",
        "🚗  Vehicles",
        "📅  Schedules",
        "🔧  Service Details",
        "📋  Complaints",
        "📊  Reports",
        "👤  Users"
    };

    public MainForm()
    {
        InitUI();
        ShowDashboard();
    }

    private void InitUI()
    {
        Text            = "Vehicle Service Management System";
        Size            = new Size(1180, 720);
        MinimumSize     = new Size(900, 600);
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = Color.FromArgb(240, 243, 248);

        // ── Sidebar ──────────────────────────────────────────────
        pnlSidebar = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 210,
            BackColor = UIHelper.Sidebar
        };

        // Brand
        var brand = new Label
        {
            Text      = "🔧 VSMS",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Top,
            Height    = 60,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = UIHelper.PrimaryDark
        };
        pnlSidebar.Controls.Add(brand);

        // User badge
        var badge = new Label
        {
            Text      = $"  👤 {Session.FullName}\n  {Session.Role}",
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(180, 200, 220),
            Dock      = DockStyle.Top,
            Height    = 50,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(10, 0, 0, 0)
        };
        pnlSidebar.Controls.Add(badge);

        // Nav buttons
        var navPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoScroll    = false,
            BackColor     = UIHelper.Sidebar,
            Padding       = new Padding(0, 10, 0, 0)
        };

        for (int i = 0; i < NavItems.Length; i++)
        {
            if (NavItems[i] == "👤  Users" && !Session.IsAdmin) continue;

            var btn = new Button
            {
                Text      = NavItems[i],
                Size      = new Size(210, 44),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 200, 220),
                BackColor = UIHelper.Sidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(18, 0, 0, 0),
                Cursor    = Cursors.Hand,
                Tag       = NavItems[i],
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = UIHelper.SidebarHov }
            };
            btn.Click += NavBtn_Click;
            navPanel.Controls.Add(btn);
        }

        // Logout button at bottom
        var btnLogout = new Button
        {
            Text      = "🚪  Logout",
            Size      = new Size(210, 44),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(220, 100, 100),
            BackColor = UIHelper.Sidebar,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(18, 0, 0, 0),
            Cursor    = Cursors.Hand,
            Dock      = DockStyle.Bottom,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = UIHelper.SidebarHov }
        };
        btnLogout.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        pnlSidebar.Controls.Add(navPanel);
        pnlSidebar.Controls.Add(btnLogout);

        // ── Top bar ──────────────────────────────────────────────
        var topBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = Color.White
        };
        lblPageTitle = new Label
        {
            Text      = "Dashboard",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = UIHelper.Dark,
            AutoSize  = false,
            Size      = new Size(600, 54),
            Location  = new Point(20, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };
        topBar.Controls.Add(lblPageTitle);

        // ── Content ──────────────────────────────────────────────
        pnlContent = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 243, 248),
            Padding   = new Padding(20)
        };

        Controls.AddRange(new Control[] { pnlContent, topBar, pnlSidebar });
    }

    private void NavBtn_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        // Reset previous active
        if (_activeBtn != null)
        {
            _activeBtn.BackColor = UIHelper.Sidebar;
            _activeBtn.ForeColor = Color.FromArgb(180, 200, 220);
            _activeBtn.Font      = new Font("Segoe UI", 10);
        }

        // Highlight active
        btn.BackColor = UIHelper.SidebarHov;
        btn.ForeColor = Color.White;
        btn.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
        _activeBtn    = btn;

        var tag = btn.Tag?.ToString() ?? "";
        lblPageTitle.Text = tag.Substring(3).Trim();

        pnlContent.Controls.Clear();

        Control page = tag switch
        {
            "🏠  Dashboard"       => new DashboardPanel(),
            "👥  Customers"       => new CustomersPanel(),
            "🚗  Vehicles"        => new VehiclesPanel(),
            "📅  Schedules"       => new SchedulesPanel(),
            "🔧  Service Details" => new ServiceDetailsPanel(),
            "📋  Complaints"      => new ComplaintsPanel(),
            "📊  Reports"         => new ReportsPanel(),
            "👤  Users"           => new UsersPanel(),
            _                     => new DashboardPanel()
        };

        page.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(page);
    }

    private void ShowDashboard()
    {
        var dashPanel = new DashboardPanel { Dock = DockStyle.Fill };
        pnlContent.Controls.Add(dashPanel);
    }
}
