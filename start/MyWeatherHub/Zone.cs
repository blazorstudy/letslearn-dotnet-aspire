namespace MyWeatherHub;
public record Zone(string Key, string Name, string State, string X, string Y);

public record Forecast(string Name, string DetailedForecast);