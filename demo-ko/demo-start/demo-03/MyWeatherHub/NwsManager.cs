using System.Text.Json;

namespace MyWeatherHub;

public class NwsManager(HttpClient client)
{
	readonly JsonSerializerOptions options = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public async Task<Zone[]> GetZonesAsync()
    {
        var response = await client.GetAsync("zones");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var zones = JsonSerializer.Deserialize<Zone[]>(content, options);

        return zones ?? [];
    }

    public async Task<Forecast[]> GetForecastByZoneAsync(Zone zone)
    {
        var response = await client.GetAsync($"forecast/{zone.Key}/{zone.X}/{zone.Y}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var forecast = JsonSerializer.Deserialize<Forecast[]>(content, options);

        return forecast ?? [];
    }
}

public record Zone(string Key, string Name, string State, string X, string Y);

public record Forecast(string Name, string DetailedForecast);