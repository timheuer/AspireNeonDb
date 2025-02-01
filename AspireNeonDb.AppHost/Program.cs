var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(c => c.WithImageTag("8.14"))
    .WithPgWeb()
    .WithBindMount("data/postgres", "/docker-entrypoint-initdb.d")
    .WithEnvironment("POSTGRES_DB","Todos");

var pgdb = postgres.AddDatabase("postgresdb", "Todos");

var neon = builder.AddContainer("neon", "timowilhelm/local-neon-http-proxy")
    .WithImageRegistry("ghcr.io")
    .WithImageTag("main")
    .WaitFor(postgres)
    .WithEnvironment(config =>
    {
        var hostname = postgres.Resource.PrimaryEndpoint.Host;
        var port = postgres.Resource.PrimaryEndpoint.Port;
        var password = postgres.Resource.PasswordParameter.Value;
        var dbname = pgdb.Resource.DatabaseName;
        var cnstr = $"postgres://postgres:{password}@{hostname}:{port}/{dbname}";
        config.EnvironmentVariables.Add("PG_CONNECTION_STRING", cnstr);
    });

var apiService = builder.AddProject<Projects.AspireNeonDb_ApiService>("apiservice")
    .WithReference(pgdb);

builder.AddProject<Projects.AspireNeonDb_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
