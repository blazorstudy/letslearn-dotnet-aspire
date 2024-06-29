﻿using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;

namespace MyWeatherHub.Data
{

    public class NwsManager(HttpClient httpClient, IMemoryCache cache)
    {
        JsonSerializerOptions options = new() 
        {
            PropertyNameCaseInsensitive = true
        };
        
        public async Task<Zone[]?> GetZonesAsync()
        {
            // 일단 전체를 불러오자

            var list = File.ReadLines("wwwroot/gisData.csv")
                                .Skip(1)
                                .Select(line => line.Split(','))
                                .Where(parts => parts[4] != string.Empty)
                                .Select(parts => new Zone(parts[1], $"{parts[3]} {parts[4]}", parts[2], parts[5], parts[6])).ToArray();

            return list;

            // To get the live zone data from NWS, uncomment the following code and comment out the return statement below
            //var response = await httpClient.GetAsync("https://api.weather.gov/zones?type=forecast");
            //response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync();
            //return JsonSerializer.Deserialize<ZonesResponse>(content, options);

            //return await cache.GetOrCreateAsync("zones", async entry =>
            //{
            //    if (entry is null)
            //        return [];

            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            //    // Deserialize the zones.json file from the wwwroot folder
            //    using var zonesJson = File.Open("wwwroot/zones.json", FileMode.Open);
            //    if (zonesJson is null)
            //        return [];

            //    var zones = await JsonSerializer.DeserializeAsync<ZonesResponse>(zonesJson, options);

            //    return zones?.Features
            //                ?.Where(f => f.Properties?.ObservationStations?.Count > 0)
            //                .Select(f => (Zone)f)
            //                .Distinct()
            //                .ToArray() ?? [];
            //});

        }

        int forecastCount = 0;
        public async Task<Forecast[]> GetForecastByZoneAsync(string zoneId)
        {

            forecastCount++;
            if (forecastCount % 5 == 0)
            {
                throw new Exception("Random exception thrown by NwsManager.GetForecastAsync");
            }

            var response = await httpClient.GetAsync($"https://api.weather.gov/zones/forecast/{zoneId}/forecast");
            response.EnsureSuccessStatusCode(); 
            var forecasts = await response.Content.ReadFromJsonAsync<ForecastResponse>(options);
            return forecasts?.Properties?.Periods?.Select(p => (Forecast)p).ToArray() ?? [];
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
