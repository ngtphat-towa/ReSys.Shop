var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("shopdb");

builder.AddProject<Projects.ReSys_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();