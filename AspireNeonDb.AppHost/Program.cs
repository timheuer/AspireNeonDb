var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb()
    .WithBindMount("data/postgres", "/docker-entrypoint-initdb.d")
    .WithEnvironment("POSTGRES_DB","Todos");

var pgdb = postgres.AddDatabase("postgresdb", "Todos");

var neon = builder.AddNeon("neon", userName:postgres.Resource.UserNameParameter, password:postgres.Resource.PasswordParameter)
    .WaitFor(postgres)
    .WithEnvironment(config =>
    {
        var hostname = postgres.Resource.PrimaryEndpoint.Host;
        var port = postgres.Resource.PrimaryEndpoint.Port;
        var password = postgres.Resource.PasswordParameter.Value;
        var dbname = pgdb.Resource.DatabaseName;
        var cnstr = $"postgresql://postgres:{password}@{hostname}:{port}/{dbname}";
        config.EnvironmentVariables.Add("PG_CONNECTION_STRING", cnstr);
    }); //https://neon.tech/guides/local-development-with-neon

var neondb = neon.AddDatabase("neondb", "Todos");

var apiService = builder.AddProject<Projects.AspireNeonDb_ApiService>("apiservice")
    .WithReference(neondb)
    .WithReference(pgdb);

builder.AddProject<Projects.AspireNeonDb_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
