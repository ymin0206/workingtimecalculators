using System;
using System.IO;
using System.Windows.Forms;

namespace 근무시간계산기;

public partial class Form1 : Form
{
    private int[] _needHours = new int[12];
    private int[] _workingDays = new int[12];
    private int _startHour = 10;
    private int _startMin = 0;
    private string _dataFilePath = "";
    private string _dataDir = "";
    private int _autoWorkedDays = 0;

    public Form1()
    {
        InitializeComponent();
        LoadConfig();
    }

    // ─── 설정 파일 로드 ──────────────────────────────────────────────────────

    private void LoadConfig()
    {
        // SingleFile 배포 시 BaseDirectory는 임시 압축해제 폴더를 가리키므로
        // 실제 exe가 있는 폴더를 기준으로 사용
        _dataDir = Path.GetDirectoryName(Environment.ProcessPath)
                   ?? AppDomain.CurrentDomain.BaseDirectory;
        _dataFilePath = Path.Combine(_dataDir, "TotalTimeList.txt");

        if (!File.Exists(_dataFilePath))
        {
            CreateDefaultFile();
            MessageBox.Show(
                $"TotalTimeList.txt 파일이 없어 새로 만들었습니다.\n\n{_dataFilePath}\n\n내용 수정 후 [다시 읽기]를 누르세요.",
                "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblFilePath.Text = _dataFilePath;
            return;
        }

        if (!ParseConfigFile(_dataFilePath, out string error))
        {
            MessageBox.Show($"파일 읽기 오류: {error}\n\n{_dataFilePath}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        RefreshMonthInfo();
        RecalcWorkedDays();
    }

    private void CreateDefaultFile()
    {
        File.WriteAllText(_dataFilePath,
            "// 월별 필수 근무 시간 (1~12월, 쉼표 구분)\r\n" +
            "172,131,168,176,152,168,184,160,160,160,168,184\r\n\r\n" +
            "// 월별 총 근무일 수 (1~12월, 쉼표 구분)\r\n" +
            "22,17,21,22,19,21,23,20,20,20,21,23\r\n\r\n" +
            "// 일반 출근 시각 (hh:mm)\r\n" +
            "10:00\r\n");
    }

    private bool ParseConfigFile(string path, out string error)
    {
        error = "";
        var lines = File.ReadAllLines(path);
        int[]? hours = null, days = null, start = null;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            if (hours == null)
            {
                hours = ParseIntArray(line, ',');
                if (hours == null || hours.Length < 12) { error = "필수 근무 시간 데이터가 12개 미만입니다."; return false; }
            }
            else if (days == null)
            {
                days = ParseIntArray(line, ',');
                if (days == null || days.Length < 12) { error = "근무일 수 데이터가 12개 미만입니다."; return false; }
            }
            else if (start == null)
            {
                start = ParseIntArray(line, ':');
                if (start == null || start.Length < 2) { error = "출근 시각 형식이 올바르지 않습니다."; return false; }
            }
        }

        if (hours == null || days == null || start == null)
        { error = "파일 형식이 올바르지 않습니다."; return false; }

        _needHours = hours;
        _workingDays = days;
        _startHour = start[0];
        _startMin = start[1];
        return true;
    }

    private static int[]? ParseIntArray(string s, char delim)
    {
        var parts = s.Split(delim);
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            if (!int.TryParse(parts[i].Trim(), out result[i])) return null;
        return result;
    }

    private void RefreshMonthInfo()
    {
        int idx = DateTime.Now.Month - 1;
        lblMonthTitle.Text = $"📅  {DateTime.Now.Year}년 {DateTime.Now.Month}월";
        lblNeedHours.Text = $"필수 근무: {_needHours[idx]}시간";
        lblTotalDays.Text = $"총 근무일: {_workingDays[idx]}일";
        txtStartTime.Text = $"{_startHour:D2}:{_startMin:D2}";
        lblFilePath.Text = _dataFilePath;
    }

    // ─── 기준 출근 시각 편집 ──────────────────────────────────────────────────

    private void txtStartTime_Leave(object sender, EventArgs e) => ApplyStartTime();
    private void txtStartTime_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) ApplyStartTime();
    }

    private void ApplyStartTime()
    {
        var parts = txtStartTime.Text.Trim().Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m) &&
            h >= 0 && h < 24 && m >= 0 && m < 60)
        {
            _startHour = h;
            _startMin = m;
            SaveConfigFile();
        }
        else
        {
            MessageBox.Show("출근 시각을 hh:mm 형식으로 입력하세요.\n예) 09:30", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtStartTime.Text = $"{_startHour:D2}:{_startMin:D2}";
        }
    }

    private void SaveConfigFile()
    {
        if (!File.Exists(_dataFilePath)) return;
        string content =
            "// 월별 필수 근무 시간 (1~12월, 쉼표 구분)\r\n" +
            string.Join(",", _needHours) + "\r\n\r\n" +
            "// 월별 총 근무일 수 (1~12월, 쉼표 구분)\r\n" +
            string.Join(",", _workingDays) + "\r\n\r\n" +
            "// 일반 출근 시각 (hh:mm)\r\n" +
            $"{_startHour:D2}:{_startMin:D2}\r\n";
        File.WriteAllText(_dataFilePath, content);
    }

    // ─── 오늘까지 출근일 자동 계산 ────────────────────────────────────────────

    private void RecalcWorkedDays()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var upTo = chkIncludeToday.Checked ? today : today.AddDays(-1);
        var cached = HolidayService.LoadCache(_dataDir, today.Year);

        _autoWorkedDays = HolidayService.CalcWorkedDaysUpToDate(upTo, cached);

        string todaySuffix = chkIncludeToday.Checked ? " (오늘 포함)" : " (오늘 미포함)";
        string holidaySuffix = cached != null ? "" : "  ⚠ 공휴일 미반영";
        lblWorkedDayVal.Text = $"{_autoWorkedDays}일{todaySuffix}{holidaySuffix}";
    }

    // ─── 계산 버튼 ────────────────────────────────────────────────────────────

    private void btnCalc_Click(object sender, EventArgs e)
    {
        int idx = DateTime.Now.Month - 1;
        int needTotalMin = _needHours[idx] * 60;
        int totalDays = _workingDays[idx];
        int workStartMin = _startHour * 60 + _startMin;

        if (!int.TryParse(txtFutureLeave.Text.Trim(), out int futureLeave) || futureLeave < 0)
        {
            ShowInputError("미래 연차 개수를 0 이상의 숫자로 입력하세요.", txtFutureLeave); return;
        }

        int nWorkingDay = _autoWorkedDays + futureLeave;

        var tp = txtTotalTime.Text.Trim().Split(':');
        if (tp.Length != 2 || !int.TryParse(tp[0], out int tHour) || !int.TryParse(tp[1], out int tMin) || tMin >= 60 || tMin < 0)
        {
            ShowInputError("총 근로 시간을 hh:mm 형식으로 입력하세요.\n예) 141:30", txtTotalTime); return;
        }

        // 미래 연차는 8시간으로 합산
        int nTotalMin = tHour * 60 + tMin + futureLeave * 8 * 60;
        int nRemainDay = totalDays - nWorkingDay;

        if (nRemainDay < 0)
        {
            ShowInputError($"출근일 합산({nWorkingDay}일)이 총 근무일({totalDays}일)을 초과합니다.\n미래 연차를 줄여주세요.", txtFutureLeave); return;
        }

        // 필수 - 실제(연차포함) - 남은일×8h  →  양수=모자람, 음수=여유
        int remainMin = needTotalMin - nTotalMin - nRemainDay * 8 * 60;
        bool bDeficit = remainMin > 0;
        int absRemain = Math.Abs(remainMin);

        panelResult.Visible = true;
        lblRemainDayVal.Text = $"{nRemainDay}일";
        lblOverLabel.Text = bDeficit ? "모자란 총 시간" : "남는 총 시간";
        lblOverTimeVal.Text = $"{absRemain / 60}시간 {absRemain % 60}분";

        if (remainMin == 0)
        {
            lblPerDayVal.Text = "8시간 근무 기준 정확히 맞음 ✔";
            lblRecommendVal.Text = "-";
            return;
        }

        if (nRemainDay > 0)
        {
            int perDay = (int)Math.Ceiling((double)absRemain / nRemainDay);
            string perDayText = $"{perDay / 60}시간 {perDay % 60}분";

            if (bDeficit)
            {
                lblPerDayVal.Text = $"{perDayText} 더 일해야 함";
                int recStart = workStartMin - perDay;
                lblRecommendVal.Text = recStart >= 0
                    ? $"{recStart / 60}시 {recStart % 60:D2}분"
                    : "-";
            }
            else
            {
                lblPerDayVal.Text = $"{perDayText} 여유 있음";
                lblRecommendVal.Text = "-";
            }
        }
        else
        {
            lblPerDayVal.Text = bDeficit
                ? $"총 {absRemain / 60}시간 {absRemain % 60}분 부족 (남은 근무일 없음)"
                : $"총 {absRemain / 60}시간 {absRemain % 60}분 여유";
            lblRecommendVal.Text = "-";
        }

        // ── 오늘이 마지막 근무일인 경우 퇴근 가능 시각 ──────────────────────
        // nRemainDay == 1: 오늘 하루만 남은 상태 (오늘 미포함으로 계산했을 때)
        if (nRemainDay == 1)
        {
            // 오늘 일해야 할 분 = 전체 필요 - 지금까지 일한 시간
            // remainMin = needTotal - nTotal - 1*8h  →  오늘 필요 = 8h + remainMin
            int todayNeedMin = 8 * 60 + remainMin;
            if (todayNeedMin <= 0)
            {
                // 이미 충분히 일했음
                lblLastDayCaption.Visible = true;
                lblLastDayVal.Visible = true;
                lblLastDayVal.Text = "이미 필수 시간 달성! 언제든 퇴근 가능 ✔";
            }
            else
            {
                int leaveMin = workStartMin + todayNeedMin + 60; // +1시간 점심
                lblLastDayCaption.Visible = true;
                lblLastDayVal.Visible = true;
                lblLastDayVal.Text = leaveMin < 24 * 60
                    ? $"{leaveMin / 60}시 {leaveMin % 60:D2}분  ({todayNeedMin / 60}시간 {todayNeedMin % 60}분 근무 + 점심 1시간)"
                    : $"자정 초과 ({todayNeedMin / 60}시간 {todayNeedMin % 60}분 필요)";
            }
        }
        else
        {
            lblLastDayCaption.Visible = false;
            lblLastDayVal.Visible = false;
        }
    }

    // ─── 공휴일 자동 갱신 ────────────────────────────────────────────────────

    private const string DefaultGovApiKey =
        "1c96535a80f1f8d5aa88b0aaf3420177575c297d7e239aa2f5ab9893c3a14c3f";

    private async void btnHoliday_Click(object sender, EventArgs e)
    {
        int year = DateTime.Now.Year;
        btnHoliday.Enabled = false;
        btnHoliday.Text = "가져오는 중...";

        try
        {
            string? apiKey = HolidayService.LoadApiKey(_dataDir);
            List<DateOnly>? holidays = null;
            string sourceLabel = "";

            // ── API 키 없으면 선택 ────────────────────────────────────────────
            if (apiKey == null)
            {
                var choice = MessageBox.Show(
                    "data.go.kr API 키가 없습니다.\n\n" +
                    "[예]       기본 키 사용\n" +
                    "[아니오]  직접 입력\n" +
                    "[취소]     nager.at 사용",
                    "API 키 선택",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (choice == DialogResult.Cancel)
                {
                    // nager.at 바로 사용
                    holidays = await HolidayService.FetchHolidaysAsync(year);
                    sourceLabel = "date.nager.at";
                }
                else if (choice == DialogResult.Yes)
                {
                    apiKey = DefaultGovApiKey;
                    HolidayService.SaveApiKey(_dataDir, apiKey);
                }
                else // 아니오 → 직접 입력
                {
                    string key = PromptInput("data.go.kr API 키 입력",
                        "data.go.kr에서 발급받은 서비스 키를 입력하세요.\n(공공데이터포털 → 한국천문연구원_특일 정보)");
                    if (string.IsNullOrWhiteSpace(key)) return;
                    HolidayService.SaveApiKey(_dataDir, key);
                    apiKey = key;
                }
            }

            // ── gov API 호출 (실패 시 재입력 or nager 전환) ────────────────────
            while (holidays == null)
            {
                try
                {
                    holidays = await HolidayService.FetchHolidaysFromGovAsync(apiKey!, year);
                    sourceLabel = "data.go.kr (공식)";
                }
                catch (Exception ex2)
                {
                    var retry = MessageBox.Show(
                        $"API 호출 실패:\n{ex2.Message}\n\n" +
                        "[예]       API 키 다시 입력\n" +
                        "[아니오]  nager.at으로 계속",
                        "API 오류", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (retry == DialogResult.Yes)
                    {
                        string key = PromptInput("API 키 재입력",
                            "올바른 data.go.kr API 키를 입력하세요.");
                        if (string.IsNullOrWhiteSpace(key)) return;
                        HolidayService.SaveApiKey(_dataDir, key);
                        apiKey = key;
                        // holidays == null → 루프 재시도
                    }
                    else
                    {
                        holidays = await HolidayService.FetchHolidaysAsync(year);
                        sourceLabel = "date.nager.at";
                    }
                }
            }

            // ── 결과 확인 & 저장 ──────────────────────────────────────────────
            var workDays = HolidayService.CalcWorkingDaysPerMonth(year, holidays);
            var reqHours = HolidayService.CalcRequiredHoursPerMonth(workDays);

            string preview = $"{year}년 공휴일 기준 근무일 / 필수 시간  [{sourceLabel}]\n\n";
            for (int m = 1; m <= 12; m++)
                preview += $"  {m,2}월: {workDays[m - 1]}일  ({reqHours[m - 1]}시간)\n";
            preview += $"\n공휴일 {holidays.Count}개 반영됨\n\nTotalTimeList.txt를 갱신할까요?";

            if (MessageBox.Show(preview, "공휴일 갱신 확인",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _needHours = reqHours;
            _workingDays = workDays;
            SaveConfigFile();
            HolidayService.SaveCache(_dataDir, year, holidays);

            RefreshMonthInfo();
            RecalcWorkedDays();
            panelResult.Visible = false;
            MessageBox.Show($"{year}년 데이터로 갱신 완료!\n출처: {sourceLabel}",
                "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"공휴일 데이터를 가져오지 못했습니다.\n\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnHoliday.Enabled = true;
            btnHoliday.Text = "🗓 공휴일 자동 갱신";
        }
    }

    // ─── 파일 열기 / 다시 읽기 ────────────────────────────────────────────────

    private void btnOpenFile_Click(object sender, EventArgs e)
    {
        if (File.Exists(_dataFilePath))
            System.Diagnostics.Process.Start("notepad.exe", _dataFilePath);
        else
            MessageBox.Show("파일이 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void btnReload_Click(object sender, EventArgs e)
    {
        panelResult.Visible = false;
        LoadConfig();
    }

    // ─── 유틸 ─────────────────────────────────────────────────────────────────

    private static void ShowInputError(string msg, Control focus)
    {
        MessageBox.Show(msg, "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus.Focus();
    }

    private static string PromptInput(string title, string prompt)
    {
        using var form = new Form
        {
            Text = title, Width = 460, Height = 160,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false, MinimizeBox = false
        };
        var lbl = new Label { Text = prompt, Left = 12, Top = 10, Width = 420, Height = 40 };
        var txt = new TextBox { Left = 12, Top = 56, Width = 420 };
        var btn = new Button
        {
            Text = "확인", Left = 352, Top = 84, Width = 80,
            DialogResult = DialogResult.OK
        };
        form.Controls.AddRange([lbl, txt, btn]);
        form.AcceptButton = btn;
        return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : "";
    }
}
