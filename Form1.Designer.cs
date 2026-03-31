namespace 근무시간계산기;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private Label lblMonthTitle;
    private Label lblNeedHours;
    private Label lblTotalDays;
    private Label lblStartCaption;
    private TextBox txtStartTime;
    private Label lblFilePath;
    private Panel panelInfo;
    private Panel panelInputs;
    private Label lblWorkedDayCaption;
    private Label lblWorkedDayVal;
    private CheckBox chkIncludeToday;
    private Label lblFutureLeaveCaption;
    private TextBox txtFutureLeave;
    private Label lblTotalTimeCaption;
    private TextBox txtTotalTime;
    private Button btnCalc;
    private Button btnOpenFile;
    private Button btnReload;
    private Button btnHoliday;
    private Panel panelResult;
    private Label lblResultTitle;
    private Label lblRemainDayCaption;
    private Label lblRemainDayVal;
    private Label lblOverLabel;
    private Label lblOverTimeVal;
    private Label lblPerDayCaption;
    private Label lblPerDayVal;
    private Label lblRecommendCaption;
    private Label lblRecommendVal;
    private Label lblLastDayCaption;
    private Label lblLastDayVal;

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        SuspendLayout();

        // ── Form ─────────────────────────────────────────────────────────────
        // 너비 560으로 여유 확보
        this.Text = "근무시간 계산기";
        this.ClientSize = new System.Drawing.Size(560, 704);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
        this.Font = new System.Drawing.Font("맑은 고딕", 9.5f);

        // ── Month title ───────────────────────────────────────────────────────
        lblMonthTitle = new Label
        {
            Text = "📅  날짜 로딩중...",
            Font = new System.Drawing.Font("맑은 고딕", 15f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(40, 60, 130),
            Location = new System.Drawing.Point(20, 16),
            Size = new System.Drawing.Size(400, 32),
            AutoSize = false
        };

        // ── Info panel ────────────────────────────────────────────────────────
        // 너비 528 (16~544)
        panelInfo = new Panel
        {
            Location = new System.Drawing.Point(16, 56),
            Size = new System.Drawing.Size(528, 38),
            BackColor = System.Drawing.Color.FromArgb(224, 232, 252),
        };
        panelInfo.Paint += (s, e) =>
            e.Graphics.DrawRectangle(
                new System.Drawing.Pen(System.Drawing.Color.FromArgb(180, 200, 240)),
                0, 0, panelInfo.Width - 1, panelInfo.Height - 1);

        lblNeedHours = new Label
        {
            Text = "필수 근무: -",
            Location = new System.Drawing.Point(10, 8),
            Size = new System.Drawing.Size(150, 22),  // 여유 있게
            ForeColor = System.Drawing.Color.FromArgb(50, 60, 100),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold)
        };
        lblTotalDays = new Label
        {
            Text = "총 근무일: -",
            Location = new System.Drawing.Point(168, 8),
            Size = new System.Drawing.Size(130, 22),
            ForeColor = System.Drawing.Color.FromArgb(50, 60, 100),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold)
        };
        lblStartCaption = new Label
        {
            Text = "기준 출근:",
            Location = new System.Drawing.Point(306, 9),
            Size = new System.Drawing.Size(70, 20),
            ForeColor = System.Drawing.Color.FromArgb(50, 60, 100),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        txtStartTime = new TextBox
        {
            Text = "10:00",
            Location = new System.Drawing.Point(378, 8),
            Size = new System.Drawing.Size(140, 22),
            TextAlign = HorizontalAlignment.Center,
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = System.Drawing.Color.FromArgb(240, 245, 255)
        };
        txtStartTime.Leave += txtStartTime_Leave;
        txtStartTime.KeyDown += txtStartTime_KeyDown;

        panelInfo.Controls.Add(lblNeedHours);
        panelInfo.Controls.Add(lblTotalDays);
        panelInfo.Controls.Add(lblStartCaption);
        panelInfo.Controls.Add(txtStartTime);

        // ── File path ─────────────────────────────────────────────────────────
        lblFilePath = new Label
        {
            Text = "",
            Location = new System.Drawing.Point(20, 100),
            Size = new System.Drawing.Size(520, 18),
            ForeColor = System.Drawing.Color.Gray,
            Font = new System.Drawing.Font("맑은 고딕", 7.5f)
        };

        var sep1 = new Label
        {
            Location = new System.Drawing.Point(16, 124),
            Size = new System.Drawing.Size(528, 1),
            BackColor = System.Drawing.Color.FromArgb(200, 210, 230)
        };

        // ── Input panel ───────────────────────────────────────────────────────
        panelInputs = new Panel
        {
            Location = new System.Drawing.Point(16, 132),
            Size = new System.Drawing.Size(528, 178),
            BackColor = System.Drawing.Color.White,
        };
        panelInputs.Paint += (s, e) =>
            e.Graphics.DrawRectangle(
                new System.Drawing.Pen(System.Drawing.Color.FromArgb(210, 215, 230)),
                0, 0, panelInputs.Width - 1, panelInputs.Height - 1);

        lblWorkedDayCaption = new Label
        {
            Text = "오늘까지 출근일  (자동 계산)",
            Location = new System.Drawing.Point(14, 14),
            Size = new System.Drawing.Size(210, 20),
            ForeColor = System.Drawing.Color.FromArgb(80, 85, 100)
        };
        chkIncludeToday = new CheckBox
        {
            Text = "오늘 포함",
            Checked = true,
            Location = new System.Drawing.Point(230, 12),
            Size = new System.Drawing.Size(90, 22),
            ForeColor = System.Drawing.Color.FromArgb(60, 80, 160),
            Cursor = Cursors.Hand
        };
        chkIncludeToday.CheckedChanged += (s, e) => RecalcWorkedDays();
        lblWorkedDayVal = new Label
        {
            Text = "- 일",
            Location = new System.Drawing.Point(14, 36),
            Size = new System.Drawing.Size(500, 22),   // 충분히 넓게
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(40, 60, 130)
        };

        lblFutureLeaveCaption = new Label
        {
            Text = "미래 연차 개수  (8시간 기준 1일)",
            Location = new System.Drawing.Point(14, 64),
            Size = new System.Drawing.Size(300, 20),
            ForeColor = System.Drawing.Color.FromArgb(80, 85, 100)
        };
        txtFutureLeave = new TextBox
        {
            Location = new System.Drawing.Point(14, 86),
            Size = new System.Drawing.Size(80, 24),
            PlaceholderText = "0",
            TextAlign = HorizontalAlignment.Center,
            Text = "0"
        };

        lblTotalTimeCaption = new Label
        {
            Text = "총 근로 시간  (연차 포함)  [hh:mm]",
            Location = new System.Drawing.Point(14, 118),
            Size = new System.Drawing.Size(300, 20),
            ForeColor = System.Drawing.Color.FromArgb(80, 85, 100)
        };
        txtTotalTime = new TextBox
        {
            Location = new System.Drawing.Point(14, 140),
            Size = new System.Drawing.Size(110, 24),
            PlaceholderText = "예) 151:44",
            TextAlign = HorizontalAlignment.Center
        };

        panelInputs.Controls.Add(lblWorkedDayCaption);
        panelInputs.Controls.Add(chkIncludeToday);
        panelInputs.Controls.Add(lblWorkedDayVal);
        panelInputs.Controls.Add(lblFutureLeaveCaption);
        panelInputs.Controls.Add(txtFutureLeave);
        panelInputs.Controls.Add(lblTotalTimeCaption);
        panelInputs.Controls.Add(txtTotalTime);

        // ── Buttons row 1 ─────────────────────────────────────────────────────
        btnCalc = new Button
        {
            Text = "계  산",
            Location = new System.Drawing.Point(16, 324),
            Size = new System.Drawing.Size(240, 38),
            BackColor = System.Drawing.Color.FromArgb(60, 100, 220),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("맑은 고딕", 11f, System.Drawing.FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCalc.FlatAppearance.BorderSize = 0;
        btnCalc.Click += btnCalc_Click;

        btnOpenFile = new Button
        {
            Text = "파일 열기",
            Location = new System.Drawing.Point(268, 324),
            Size = new System.Drawing.Size(138, 38),
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.FromArgb(60, 70, 100),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnOpenFile.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 190, 220);
        btnOpenFile.Click += btnOpenFile_Click;

        btnReload = new Button
        {
            Text = "다시 읽기",
            Location = new System.Drawing.Point(416, 324),
            Size = new System.Drawing.Size(128, 38),
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.FromArgb(60, 70, 100),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnReload.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 190, 220);
        btnReload.Click += btnReload_Click;

        // ── 공휴일 갱신 버튼 ──────────────────────────────────────────────────
        btnHoliday = new Button
        {
            Text = "🗓 공휴일 자동 갱신",
            Location = new System.Drawing.Point(16, 370),
            Size = new System.Drawing.Size(528, 36),
            BackColor = System.Drawing.Color.FromArgb(240, 245, 255),
            ForeColor = System.Drawing.Color.FromArgb(50, 80, 180),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnHoliday.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(160, 185, 230);
        btnHoliday.Click += btnHoliday_Click;

        var sep2 = new Label
        {
            Location = new System.Drawing.Point(16, 416),
            Size = new System.Drawing.Size(528, 1),
            BackColor = System.Drawing.Color.FromArgb(200, 210, 230)
        };

        // ── Result panel ──────────────────────────────────────────────────────
        panelResult = new Panel
        {
            Location = new System.Drawing.Point(16, 424),
            Size = new System.Drawing.Size(528, 260),
            BackColor = System.Drawing.Color.White,
            Visible = false
        };
        panelResult.Paint += (s, e) =>
            e.Graphics.DrawRectangle(
                new System.Drawing.Pen(System.Drawing.Color.FromArgb(210, 215, 230)),
                0, 0, panelResult.Width - 1, panelResult.Height - 1);

        lblResultTitle = new Label
        {
            Text = "─── 결 과 ──────────────────────────────────────────",
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(500, 20),
            Font = new System.Drawing.Font("맑은 고딕", 8.5f),
            ForeColor = System.Drawing.Color.FromArgb(140, 150, 180)
        };

        // caption 200, value 300 → 넉넉하게
        static (Label cap, Label val) MakeRow(string caption, int y)
        {
            var cap = new Label
            {
                Text = caption,
                Location = new System.Drawing.Point(16, y),
                Size = new System.Drawing.Size(200, 24),
                ForeColor = System.Drawing.Color.FromArgb(80, 85, 110),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            var val = new Label
            {
                Text = "-",
                Location = new System.Drawing.Point(220, y),
                Size = new System.Drawing.Size(296, 24),   // 여유 있게
                Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(40, 60, 130),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            return (cap, val);
        }

        var (capRemain, valRemain) = MakeRow("남은 근무일 (연차 비포함)", 40);
        lblRemainDayCaption = capRemain; lblRemainDayVal = valRemain;

        lblOverLabel = new Label
        {
            Text = "모자란 총 시간",
            Location = new System.Drawing.Point(16, 76),
            Size = new System.Drawing.Size(200, 24),
            ForeColor = System.Drawing.Color.FromArgb(80, 85, 110),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        lblOverTimeVal = new Label
        {
            Text = "-",
            Location = new System.Drawing.Point(220, 76),
            Size = new System.Drawing.Size(296, 24),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(40, 60, 130),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        var (capPerDay, valPerDay) = MakeRow("8시간 근무 기준", 112);
        lblPerDayCaption = capPerDay; lblPerDayVal = valPerDay;

        var (capRec, valRec) = MakeRow("권장 출근 시각", 152);
        lblRecommendCaption = capRec; lblRecommendVal = valRec;

        lblLastDayCaption = new Label
        {
            Text = "🏁 오늘 퇴근 가능 시각",
            Location = new System.Drawing.Point(16, 200),
            Size = new System.Drawing.Size(200, 24),
            ForeColor = System.Drawing.Color.FromArgb(160, 60, 60),
            Font = new System.Drawing.Font("맑은 고딕", 9.5f, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Visible = false
        };
        lblLastDayVal = new Label
        {
            Text = "-",
            Location = new System.Drawing.Point(220, 200),
            Size = new System.Drawing.Size(296, 24),
            Font = new System.Drawing.Font("맑은 고딕", 10f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(160, 60, 60),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Visible = false
        };

        panelResult.Controls.AddRange(new Control[] {
            lblResultTitle,
            lblRemainDayCaption, lblRemainDayVal,
            lblOverLabel, lblOverTimeVal,
            lblPerDayCaption, lblPerDayVal,
            lblRecommendCaption, lblRecommendVal,
            lblLastDayCaption, lblLastDayVal
        });

        this.Controls.AddRange(new Control[] {
            lblMonthTitle, panelInfo, lblFilePath, sep1,
            panelInputs,
            btnCalc, btnOpenFile, btnReload,
            btnHoliday, sep2, panelResult
        });

        ResumeLayout(false);
    }
}
