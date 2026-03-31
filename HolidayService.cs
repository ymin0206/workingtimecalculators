using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace 근무시간계산기;

public record HolidayItem(string Date, string LocalName, string Name);

public static class HolidayService
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(15) };

    // ─── 공개 API (date.nager.at) ──────────────────────────────────────────────

    public static async Task<List<DateOnly>> FetchHolidaysAsync(int year)
    {
        var url = $"https://date.nager.at/api/v3/publicholidays/{year}/KR";
        var items = await _client.GetFromJsonAsync<HolidayItem[]>(url)
                    ?? throw new Exception("공휴일 데이터를 받지 못했습니다.");
        var holidays = items.Select(h => DateOnly.ParseExact(h.Date, "yyyy-MM-dd")).ToList();
        return AddSubstituteHolidays(holidays);
    }

    // ─── 정부 API (data.go.kr) ─────────────────────────────────────────────────

    public static async Task<List<DateOnly>> FetchHolidaysFromGovAsync(string apiKey, int year)
    {
        var holidays = new List<DateOnly>();

        for (int month = 1; month <= 12; month++)
        {
            var url = "http://apis.data.go.kr/B090041/openapi/service/SpcdeInfoService/getRestDeInfo" +
                      $"?ServiceKey={Uri.EscapeDataString(apiKey)}" +
                      $"&solYear={year}&solMonth={month:D2}&numOfRows=50&_type=json";

            var json = await _client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var body = doc.RootElement
                          .GetProperty("response")
                          .GetProperty("body");

            if (body.GetProperty("totalCount").GetInt32() == 0) continue;

            var item = body.GetProperty("items").GetProperty("item");
            ParseGovItems(item, holidays);
        }

        return holidays.OrderBy(d => d).ToList();
    }

    private static void ParseGovItems(JsonElement item, List<DateOnly> result)
    {
        if (item.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in item.EnumerateArray())
                TryAddGovItem(el, result);
        }
        else if (item.ValueKind == JsonValueKind.Object)
        {
            TryAddGovItem(item, result);
        }
    }

    private static void TryAddGovItem(JsonElement el, List<DateOnly> result)
    {
        if (el.GetProperty("isHoliday").GetString() != "Y") return;
        int locdate = el.GetProperty("locdate").GetInt32();
        int y = locdate / 10000, m = (locdate % 10000) / 100, d = locdate % 100;
        result.Add(new DateOnly(y, m, d));
    }

    // ─── 대체공휴일 계산 (date.nager.at 전용) ──────────────────────────────────

    private static List<DateOnly> AddSubstituteHolidays(List<DateOnly> holidays)
    {
        var result = new HashSet<DateOnly>(holidays);

        foreach (var holiday in holidays)
        {
            var dow = holiday.DayOfWeek;
            if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday) continue;

            var sub = dow == DayOfWeek.Saturday ? holiday.AddDays(2) : holiday.AddDays(1);
            while (result.Contains(sub) || sub.DayOfWeek == DayOfWeek.Saturday || sub.DayOfWeek == DayOfWeek.Sunday)
                sub = sub.AddDays(1);

            result.Add(sub);
        }

        return [.. result.OrderBy(d => d)];
    }

    // ─── API 키 저장/로드 ──────────────────────────────────────────────────────

    public static string? LoadApiKey(string dir)
    {
        var path = Path.Combine(dir, "api_key.txt");
        if (!File.Exists(path)) return null;
        var key = File.ReadAllText(path).Trim();
        return string.IsNullOrEmpty(key) ? null : key;
    }

    public static void SaveApiKey(string dir, string key)
        => File.WriteAllText(Path.Combine(dir, "api_key.txt"), key.Trim());

    // ─── 공휴일 캐시 저장/로드 ────────────────────────────────────────────────

    public static void SaveCache(string dir, int year, List<DateOnly> holidays)
        => File.WriteAllLines(
            Path.Combine(dir, $"holiday_cache_{year}.txt"),
            holidays.Select(d => d.ToString("yyyy-MM-dd")));

    public static List<DateOnly>? LoadCache(string dir, int year)
    {
        var path = Path.Combine(dir, $"holiday_cache_{year}.txt");
        if (!File.Exists(path)) return null;
        try
        {
            return File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => DateOnly.ParseExact(l.Trim(), "yyyy-MM-dd"))
                .ToList();
        }
        catch { return null; }
    }

    // ─── 근무일 계산 ──────────────────────────────────────────────────────────

    public static int CalcWorkedDaysUpToDate(DateOnly upTo, List<DateOnly>? holidays)
    {
        var set = holidays != null ? new HashSet<DateOnly>(holidays) : [];
        int count = 0;
        for (var d = new DateOnly(upTo.Year, upTo.Month, 1); d <= upTo; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !set.Contains(d))
                count++;
        return count;
    }

    public static int[] CalcWorkingDaysPerMonth(int year, List<DateOnly> holidays)
    {
        var set = new HashSet<DateOnly>(holidays);
        var result = new int[12];
        for (int m = 1; m <= 12; m++)
            for (int day = 1; day <= DateTime.DaysInMonth(year, m); day++)
            {
                var d = new DateOnly(year, m, day);
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !set.Contains(d))
                    result[m - 1]++;
            }
        return result;
    }

    public static int[] CalcRequiredHoursPerMonth(int[] workingDays)
        => workingDays.Select(d => d * 8).ToArray();
}
