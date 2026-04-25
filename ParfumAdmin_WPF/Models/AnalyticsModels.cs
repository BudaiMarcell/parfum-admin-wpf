using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class AnalyticsOverview
    {
        [JsonPropertyName("today")]      public AnalyticsPeriod Today      { get; set; }
        [JsonPropertyName("this_week")]  public AnalyticsPeriod ThisWeek   { get; set; }
        [JsonPropertyName("this_month")] public AnalyticsPeriod ThisMonth  { get; set; }
    }

    public class AnalyticsPeriod
    {
        [JsonPropertyName("pageviews")]        public int    Pageviews       { get; set; }
        [JsonPropertyName("unique_sessions")]  public int    UniqueSessions  { get; set; }
        // Egyedi látogatók: IP + eszköz fingerprint (bejelentkezett usernél user_id).
        // Több session ugyanattól a személytől = 1 látogató.
        [JsonPropertyName("unique_visitors")]  public int    UniqueVisitors  { get; set; }
        [JsonPropertyName("add_to_carts")]     public int    AddToCarts      { get; set; }
        [JsonPropertyName("checkouts")]        public int    Checkouts       { get; set; }
        [JsonPropertyName("orders")]           public int    Orders          { get; set; }
        [JsonPropertyName("new_visitors")]     public int    NewVisitors     { get; set; }
        [JsonPropertyName("revenue")]          public double Revenue         { get; set; }
    }

    public class DailyPoint
    {
        [JsonPropertyName("date")]      public string Date      { get; set; }
        [JsonPropertyName("pageviews")] public int    Pageviews { get; set; }
        [JsonPropertyName("sessions")]  public int    Sessions  { get; set; }
        [JsonPropertyName("orders")]    public int    Orders    { get; set; }
        [JsonPropertyName("revenue")]   public double Revenue   { get; set; }
    }

    public class DailySeries
    {
        [JsonPropertyName("labels")] public List<string>    Labels { get; set; }
        [JsonPropertyName("series")] public List<DailyPoint> Series { get; set; }
    }

    public class HourlySeries
    {
        [JsonPropertyName("labels")] public List<int> Labels { get; set; }
        [JsonPropertyName("data")]   public List<int> Data   { get; set; }
    }

    public class DeviceStats
    {
        [JsonPropertyName("desktop")] public int Desktop { get; set; }
        [JsonPropertyName("mobile")]  public int Mobile  { get; set; }
        [JsonPropertyName("tablet")]  public int Tablet  { get; set; }
    }

    public class FunnelStats
    {
        [JsonPropertyName("pageviews")]    public int Pageviews    { get; set; }
        [JsonPropertyName("add_to_carts")] public int AddToCarts   { get; set; }
        [JsonPropertyName("checkouts")]    public int Checkouts    { get; set; }
        [JsonPropertyName("orders")]       public int Orders       { get; set; }
    }
}
