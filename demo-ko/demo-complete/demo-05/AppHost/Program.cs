var builder = DistributedApplication.CreateBuilder(args);
var cache = builder.AddRedis("cache");
var apiService = builder.AddProject<Projects.Api>("api")
    .WithReference(cache);

builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
		.WithReference(apiService);

builder.Build().Run();
