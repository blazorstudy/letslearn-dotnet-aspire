var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Api>("api");
builder.AddProject<Projects.MyWeatherHub>("myweatherhub");

builder.Build().Run();
