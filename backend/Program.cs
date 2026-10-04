using Dapper;
using DotNetEnv;
using Npgsql;

Env.Load(); // reads .env into process environment variables

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL is not set.");
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString)); // One data source for the whole app; Npgsql handles pooling internally
builder.Services.AddProblemDetails(); // for errorhandling
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowAll");

// routes
// etnisitetskartet.no
app.MapGet("/kartpop", async (NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    var municipalities = await conn.QueryAsync<Municipality>("SELECT * FROM komlatlong");
    return Results.Ok(municipalities);
});

// syncit2.com
//      videos3
app.MapGet("/videos3/songs", async (NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    var songs = await conn.QueryAsync(
        "SELECT DISTINCT songname, creator FROM videos3 ORDER BY songname");
    return Results.Ok(songs);
});

app.MapGet("/videos3/{songname}", async (string songname, NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    var rows = await conn.QueryAsync("""
        SELECT * FROM videos3
        WHERE songname = @songname
        ORDER BY
            CASE row_position WHEN 'top' THEN 0 WHEN 'bot' THEN 1 ELSE 2 END,
            column_position
        """, new { songname });
    return Results.Ok(rows);
});

//      songsrequested
app.MapGet("/songsRequested/songs", async (NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    var ids = await conn.QueryAsync("SELECT DISTINCT randomsongid FROM songsrequested");
    return Results.Ok(ids);
});

app.MapPost("/songsRequested/newSong", async (SongRequest[] rows, NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    await using var tx = await conn.BeginTransactionAsync(); // all rows or none

    await conn.ExecuteAsync("""
        INSERT INTO songsrequested
            (randomsongid, youtube_id, start_time, row_position, column_position, creator)
        VALUES
            (@randomsongid, @youtube_id, @start_time, @row_position, @column_position, @creator)
        """, rows, tx);

    await tx.CommitAsync();

    return Results.Created("/songsRequested/songs", new { message = "Rows inserted successfully" });
});

app.Run($"http://0.0.0.0:{port}");

public record Municipality(int Id, string Ssbid, string Kommune, double Lat, double Long);

// Property names match the SQL parameter names used by Dapper
public record SongRequest(
    int randomsongid,
    string youtube_id,
    float start_time,
    string row_position,
    int column_position,
    string creator
);