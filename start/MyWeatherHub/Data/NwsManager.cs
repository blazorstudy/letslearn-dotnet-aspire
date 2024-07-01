using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Web;

namespace MyWeatherHub.Data
{

    public class NwsManager(HttpClient httpClient, IMemoryCache cache)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public IEnumerable<Zone> GetZones()
        {
            // 일단 전체를 불러오자

            return File.ReadLines("wwwroot/gisData.csv")
                                .Skip(1)
                                .Select(line => line.Split(','))
                                .Where(parts => parts[4] != string.Empty)
                                .Select(parts => new Zone(parts[1], $"{parts[3]} {parts[4]}", parts[2], parts[5], parts[6]));
        }

        int forecastCount = 0;

        public async Task<Forecast[]> GetForecastByZoneAsync(Zone zone)
        {

            forecastCount++;

            if (forecastCount % 5 == 0)
            {
                throw new Exception("Random exception thrown by NwsManager.GetForecastAsync");
            }

            // Base_time : 0200, 0500, 0800, 1100, 1400, 1700, 2000, 2300 (1일 8회)
            var queryDate = DateTime.Now.Hour >= 11 ? DateTime.Now : DateTime.Now - TimeSpan.FromDays(1);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["ServiceKey"] = Environment.GetEnvironmentVariable("WeatherForcastServiceKey");
            query["pageNo"] = "1";
            query["numOfRows"] = "3000";
            query["dataType"] = "json";
            query["base_date"] = $"{queryDate:yyyyMMdd}";
            query["base_time"] = "1100";
            query["nx"] = $"{zone.X}";
            query["ny"] = $"{zone.Y}";

            var response = await httpClient.GetAsync($"http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst?{query}").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var forecasts = await response.Content.ReadFromJsonAsync<ForecastResponse>(options).ConfigureAwait(false);

            return forecasts?.Response?.Body?.Items?.GetForecast().ToArray() ?? [];
        }

    }

}

namespace Microsoft.Extensions.DependencyInjection
{


    public static class NwsManagerExtensions
    {

        public static IServiceCollection AddNwsManager(this IServiceCollection services)
        {
            services.AddHttpClient<MyWeatherHub.Data.NwsManager>(client =>
            {
                client.BaseAddress = new Uri("https://api.weather.gov/");
                client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - .NET Aspire Demo");
            });

            services.AddMemoryCache();

            return services;
        }

    }

}
