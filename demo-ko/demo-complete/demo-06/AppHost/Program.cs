var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Api>("api");

builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
		.WithReference(apiService);

builder.AddNpmApp( "react", "../MyReactApp")
						.WithReference(apiService)
						.WithEndpoint(targetPort: 3000, scheme: "http", env: "PORT");

builder.Build().Run();
