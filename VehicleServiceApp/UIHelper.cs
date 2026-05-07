namespace VehicleServiceApp;

/// <summary>Holds the currently logged-in user's info for the session.</summary>
public static class Session
{
    public static int    UserID   { get; set; }
    public static string FullName { get; set; } = "";
    public static string Role     { get; set; } = "";
    public static bool   IsAdmin  => Role == "Admin";
}

/// <summary>Central place for colours, fonts and reusable UI helpers.</summary>
public static class UIHelper
{
    // ── Palette ──────────────────────────────────────────────────
    public static readonly Color Primary     = Color.FromArgb(30,  144, 255); // Dodger Blue
    public static readonly Color PrimaryDark = Color.FromArgb(0,   102, 204);
    public static readonly Color Success     = Color.FromArgb(40,  167,  69);
    public static readonly Color Danger      = Color.FromArgb(220,  53,  69);
    public static readonly Color Warning     = Color.FromArgb(255, 193,   7);
    public static readonly Color Dark        = Color.FromArgb(33,   37,  41);
    public static readonly Color Light       = Color.FromArgb(248, 249, 250);
    public static readonly Color Sidebar     = Color.FromArgb(22,   28,  36);
    public static readonly Color SidebarHov  = Color.FromArgb(40,   50,  65);
    public static readonly Color CardBg      = Color.White;

    // ── Fonts ─────────────────────────────────────────────────────
    public static readonly Font FontTitle  = new("Segoe UI", 18, FontStyle.Bold);
    public static readonly Font FontSub    = new("Segoe UI", 10, FontStyle.Regular);
    public static readonly Font FontBtn    = new("Segoe UI",  9, FontStyle.Bold);
    public static readonly Font FontLabel  = new("Segoe UI",  9, FontStyle.Regular);
    public static readonly Font FontGrid   = new("Segoe UI",  9, FontStyle.Regular);

    // ── Button factory ────────────────────────────────────────────
    public static Button MakeButton(string text, Color bg, Color fg, int w = 110, int h = 34)
    {
        return new Button
        {
            Text        = text,
            BackColor   = bg,
            ForeColor   = fg,
            FlatStyle   = FlatStyle.Flat,
            Font        = FontBtn,
            Size        = new Size(w, h),
            Cursor      = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }

    // ── DataGridView styler ───────────────────────────────────────
    public static void StyleGrid(DataGridView dgv)
    {
        dgv.BorderStyle              = BorderStyle.None;
        dgv.BackgroundColor          = Color.White;
        dgv.GridColor                = Color.FromArgb(230, 230, 230);
        dgv.DefaultCellStyle.Font    = FontGrid;
        dgv.DefaultCellStyle.BackColor = Color.White;
        dgv.DefaultCellStyle.SelectionBackColor = Primary;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(22, 28, 36);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font       = FontBtn;
        dgv.ColumnHeadersDefaultCellStyle.Padding    = new Padding(6, 0, 0, 0);
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        dgv.ColumnHeadersHeight  = 38;
        dgv.RowTemplate.Height   = 32;
        dgv.EnableHeadersVisualStyles = false;
        dgv.SelectionMode        = DataGridViewSelectionMode.FullRowSelect;
        dgv.ReadOnly             = true;
        dgv.AllowUserToAddRows   = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.RowHeadersVisible    = false;
        dgv.AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
    }

    // ── TextBox factory ───────────────────────────────────────────
    public static TextBox MakeTextBox(int w = 220, bool multiline = false, int lines = 1)
    {
        return new TextBox
        {
            Width     = w,
            Multiline = multiline,
            Lines     = multiline ? new string[lines] : Array.Empty<string>(),
            Height    = multiline ? lines * 20 + 10 : 26,
            Font      = FontLabel,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    // ── Label factory ─────────────────────────────────────────────
    public static Label MakeLabel(string text, bool bold = false)
    {
        return new Label
        {
            Text      = text,
            Font      = bold ? new Font("Segoe UI", 9, FontStyle.Bold) : FontLabel,
            AutoSize  = true,
            ForeColor = Color.FromArgb(60, 60, 60)
        };
    }

    // ── Panel card ────────────────────────────────────────────────
    public static Panel MakeCard(int x, int y, int w, int h)
    {
        return new Panel
        {
            Location  = new Point(x, y),
            Size      = new Size(w, h),
            BackColor = CardBg,
            Padding   = new Padding(16)
        };
    }

    // ── Simple hash (NOT for production – for student demo only) ──
    public static string SimpleHash(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLower();
    }
}
