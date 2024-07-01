using System.Text;

namespace MyWeatherHub.Data;

public class ForecastResponse
{
    public Response Response { get; set; }
}

public class Response
{
    public Header Header { get; set; }
    public Body Body { get; set; }
}

public class Header
{
    public string ResultCode { get; set; }
    public string ResultMsg { get; set; }
}

public class Body
{
    public string DataType { get; set; }
    public ForecastProperties Items { get; set; }
    public int PageNo { get; set; }
    public int NumOfRows { get; set; }
    public int TotalCount { get; set; }
}

public class ForecastProperties
{
    public ForecastProperty[] Item { get; set; }

    public IEnumerable<Forecast> GetForecast()
    {
        return Item?.Where(x => x.FcstTime == "0500")
                    .GroupBy(x => x.FcstDate)
                    .Select(g =>
                    {
                        var d = g.ToDictionary(x => x.Category);

                        return new Forecast(g.Key, GetDetailedForecast(d));
                    }) ?? [];
    }

    private static string GetDetailedForecast(Dictionary<string, ForecastProperty> forecastProperties)
    {
        var detailBuilder = new StringBuilder();

        if (forecastProperties.TryGetValue("TMN", out var tmn))
        {
            detailBuilder.Append($"최저기온은 {tmn.FcstValue}도, ");
        }

        if (forecastProperties.TryGetValue("TMX", out var tmx))
        {
            detailBuilder.Append($"최고기온은 {tmx.FcstValue}도, ");
        }

        if (forecastProperties.TryGetValue("TMP", out var tmp))
        {
            detailBuilder.Append($"현재기온은 {tmp.FcstValue}도, ");
        }

        if (forecastProperties.TryGetValue("PCP", out var pcp))
        {
            detailBuilder.AppendLine($"{pcp.FcstValue}");
        }

        return detailBuilder.ToString();
    }
}

public class ForecastProperty
{
    public string BaseDate { get; set; }
    public string BaseTime { get; set; }
    public string Category { get; set; }
    public string FcstDate { get; set; }
    public string FcstTime { get; set; }
    public string FcstValue { get; set; }
    public int Nx { get; set; }
    public int Ny { get; set; }
}
