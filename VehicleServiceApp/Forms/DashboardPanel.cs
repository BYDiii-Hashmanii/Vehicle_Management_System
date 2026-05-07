using VehicleServiceApp.Database;

namespace VehicleServiceApp.Forms;

public class DashboardPanel : Panel
{
    public DashboardPanel()
    {
        BackColor = Color.FromArgb(240, 243, 248);
        Dock      = DockStyle.Fill;
        Padding   = new Padding(10);
        Build();
    }

    private void Build()
    {
        var greeting = new Label
        {
            Text      = $"Welcome back, {Session.FullName}! 👋",
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = UIHelper.Dark,
            AutoSize  = true,
            Location  = new Point(0, 5)
        };
        Controls.Add(greeting);

        var sub = new Label
        {
            Text      = "Here's a quick overview of the system today.",
            Font      = UIHelper.FontSub,
            ForeColor = Color.Gray,
            AutoSize  = true,
            Location  = new Point(0, 35)
        };
        Controls.Add(sub);

        // ── Stat cards ────────────────────────────────────────────
        int cx   = 0, cy = 70, cw = 185, ch = 100, gap = 16;
        AddCard("👥 Customers",       GetCount("Customers"),    UIHelper.Primary,  cx, cy, cw, ch); cx += cw + gap;
        AddCard("🚗 Vehicles",        GetCount("Vehicles"),     UIHelper.Success,  cx, cy, cw, ch); cx += cw + gap;
        AddCard("📅 Schedules Today", GetTodaySchedules(),      UIHelper.Warning,  cx, cy, cw, ch); cx += cw + gap;
        AddCard("📋 Open Complaints", GetOpenComplaints(),      UIHelper.Danger,   cx, cy, cw, ch);

        // ── Recent schedules ─────────────────────────────────────
        var lbl = new Label
        {
            Text      = "Recent Service Schedules",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = UIHelper.Dark,
            AutoSize  = true,
            Location  = new Point(0, cy + ch + 20)
        };
        Controls.Add(lbl);

        var dgv = new DataGridView
        {
            Location = new Point(0, cy + ch + 48),
            Size     = new Size(820, 240),
            Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        UIHelper.StyleGrid(dgv);
        try
        {
            dgv.DataSource = DB.GetAllSchedules();
        }
        catch { }
        Controls.Add(dgv);
    }

    private void AddCard(string title, string value, Color color, int x, int y, int w, int h)
    {
        var card = new Panel
        {
            Location  = new Point(x, y),
            Size      = new Size(w, h),
            BackColor = color
        };

        var lblVal = new Label
        {
            Text      = value,
            Font      = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(w, 55),
            Location  = new Point(0, 12),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblTitle = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(230, 230, 230),
            AutoSize  = false,
            Size      = new Size(w, 30),
            Location  = new Point(0, 62),
            TextAlign = ContentAlignment.MiddleCenter
        };

        card.Controls.AddRange(new Control[] { lblVal, lblTitle });
        Controls.Add(card);
    }

    private static string GetCount(string table)
    {
        try
        {
            var dt = DB.ExecuteQuery($"SELECT COUNT(*) AS C FROM {table}");
            return dt.Rows[0]["C"].ToString() ?? "0";
        }
        catch { return "?"; }
    }

    private static string GetTodaySchedules()
    {
        try
        {
            var dt = DB.ExecuteQuery(
                "SELECT COUNT(*) AS C FROM ServiceSchedule WHERE ScheduledDate=CAST(GETDATE() AS DATE)");
            return dt.Rows[0]["C"].ToString() ?? "0";
        }
        catch { return "?"; }
    }

    private static string GetOpenComplaints()
    {
        try
        {
            var dt = DB.ExecuteQuery(
                "SELECT COUNT(*) AS C FROM Complaints WHERE Status IN ('Open','In Progress')");
            return dt.Rows[0]["C"].ToString() ?? "0";
        }
        catch { return "?"; }
    }
}
