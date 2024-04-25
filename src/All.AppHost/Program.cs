var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SampleAPI>("sampleapi");

builder.AddProject<Projects.MinimalApi>("minimalapi");

builder.Build().Run();
