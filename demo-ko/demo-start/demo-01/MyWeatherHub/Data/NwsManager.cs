using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MyWeatherHub.Data
{

    public class NwsManager(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<Zone[]?> GetZonesAsync()
        {
            var list = File.ReadLines("wwwroot/gisData.csv")
                                .Skip(1)
                                .Select(line => line.Split(','))
                                .Where(parts => parts[4] != string.Empty)
                                .Select(parts => new Zone(parts[1], $"{parts[3]} {parts[4]}", parts[2], parts[5], parts[6])).ToArray();

            return list;

        }

        int forecastCount = 0;
        public async Task<Forecast[]> GetForecastByZoneAsync(string x, string y)
        {

            forecastCount++;
            if (forecastCount % 5 == 0)
            {
                throw new Exception("Random exception thrown by NwsManager.GetForecastAsync");
            }

            var serviceKey = configuration["serviceKey"] ?? throw new InvalidOperationException("service Key is not set");

            var baseDate = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
            var baseTime = "0500";
            var numOfRows = 1000;
            var requestUrl = $"http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst?serviceKey={serviceKey}&numOfRows={numOfRows}&pageNo={1}&pageNo=1&base_date={baseDate}&base_time={baseTime}&nx={x}&ny={y}&dataType=json";

            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var forecasts = await response.Content.ReadFromJsonAsync<ForecastResponse>(options);
            return forecasts.GetForecast(baseDate);
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
