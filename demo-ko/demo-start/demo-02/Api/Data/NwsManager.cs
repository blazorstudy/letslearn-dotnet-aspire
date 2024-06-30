using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Api
{

	public class NwsManager(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
	{
		JsonSerializerOptions options = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public async Task<Zone[]?> GetZonesAsync()
		{
			return await cache.GetOrCreateAsync("zones", async entry =>
			{
				if (entry is null)
					return [];

				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

				var list = File.ReadLines("wwwroot/gisData.csv")
						.Skip(1)
						.Select(line => line.Split(','))
						.Where(parts => parts[4] != string.Empty)
						.Select(parts => new Zone(parts[1], $"{parts[3]} {parts[4]}", parts[2], parts[5], parts[6])).ToArray();

				return list;
			});
		}

		static int forecastCount = 0;
		public async Task<Forecast[]> GetForecastByZoneAsync(string x, string y)
		{
			// create an exception every 5 calls to simulate and error for testing
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
			var forecasts = await response.Content.ReadFromJsonAsync<ApiResponse>(options);
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
			services.AddHttpClient<Api.NwsManager>(client =>
			{
				client.BaseAddress = new Uri("http://apis.data.go.kr/");
				//client.DefaultRequestHeaders.Add("User-Agent", "Microsoft - .NET Aspire Demo");
			});

			services.AddMemoryCache();

			// Add default output caching
			services.AddOutputCache(options =>
			{
				options.AddBasePolicy(builder => builder.Cache());
			});

			return services;
		}

		public static WebApplication? MapApiEndpoints(this WebApplication? app)
		{
			if(app is null)
				return null;

			app.UseOutputCache();

			app.MapGet("/zones", async (Api.NwsManager manager) =>
			{
				var zones = await manager.GetZonesAsync();
				return TypedResults.Ok(zones);
			})
			.WithName("GetZones")
			.CacheOutput(policy =>
			{
				policy.Expire(TimeSpan.FromHours(1));
			})
			.WithOpenApi();

			app.MapGet("/forecast/{zoneId}/{x}/{y}", async Task<Results<Ok<Api.Forecast[]>, NotFound>> (Api.NwsManager manager, string zoneId, string x, string y) =>
			{
				try
				{
					var forecasts = await manager.GetForecastByZoneAsync(x, y);
					return TypedResults.Ok(forecasts);
				}
				catch (HttpRequestException ex)
				{
					return TypedResults.NotFound();
				}
			})
			.WithName("GetForecastByZone")
			.CacheOutput(policy =>
			{
				policy.Expire(TimeSpan.FromSeconds(3)).SetVaryByRouteValue("zoneId");
			})
			.WithOpenApi();

			return app;
		}
	}
}