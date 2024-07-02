namespace MyWeatherHub.Data;

public class ForecastResponse
{
    public Response response { get; set; }

    public Forecast[] GetForecast(string baseDate)
    {
        var result = new List<Forecast>();

        var fsctTimes = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            fsctTimes.Add($"{(i + 10).ToString("00")}00");
        }

        var items = response.body.items.item;
        foreach (var time in fsctTimes)
        {
            var tmp = items.FirstOrDefault(x => x.fcstDate == baseDate && x.fcstTime == time && x.category == "TMP")?.fcstValue;
            var pop = items.FirstOrDefault(x => x.fcstDate == baseDate && x.fcstTime == time && x.category == "POP")?.fcstValue;
            var sky = items.FirstOrDefault(x => x.fcstDate == baseDate && x.fcstTime == time && x.category == "SKY")?.fcstValue;
            var skyValue = sky switch
            {
                "1" => "맑음",
                "3" => "구름많음",
                "4" => "흐림",
                _ => "알수없음"
            };

            var title = $"{time.Substring(0, 2)}:{time.Substring(2, 2)}의 예보";
            var detail = $"{skyValue}. 기온은 {tmp}로 예상. 비올 확률은 {pop}%";
            result.Add(new Forecast(title, detail));

        }

        return result.ToArray();
    }
}

public class Response
{
    public Header header { get; set; }
    public Body body { get; set; }
}

public class Header
{
    public string resultCode { get; set; }
    public string resultMsg { get; set; }
}

public class Body
{
    public string dataType { get; set; }
    public Items items { get; set; }
    public int pageNo { get; set; }
    public int numOfRows { get; set; }
    public int totalCount { get; set; }
}

public class Items
{
    public Item[] item { get; set; }
}

public class Item
{
    public string baseDate { get; set; }
    public string baseTime { get; set; }
    public string category { get; set; }
    public string fcstDate { get; set; }
    public string fcstTime { get; set; }
    public string fcstValue { get; set; }
    public int nx { get; set; }
    public int ny { get; set; }
}
