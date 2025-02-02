using Npgsql;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("neondb");

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/todos", async (NpgsqlConnection db) =>
{
    const string sql = """
                SELECT Id, Title, IsComplete
                FROM Todos
                """;

    return await db.QueryAsync<Todo>(sql);
});

app.MapGet("/todos/{id}", async (int id, NpgsqlConnection db) =>
{
    const string sql = """
                SELECT Id, Title, IsComplete
                FROM Todos
                WHERE Id = @id
                """;

    return await db.QueryFirstOrDefaultAsync<Todo>(sql, new { id }) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound();
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

public record Todo(int Id, string Title, bool IsComplete);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
