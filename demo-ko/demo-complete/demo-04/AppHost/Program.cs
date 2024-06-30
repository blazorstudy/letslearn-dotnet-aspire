var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Api>("api");
builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
		.WithReference(apiService);

builder.Build().Run();
