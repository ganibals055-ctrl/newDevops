using Microsoft.EntityFrameworkCore;
using RatingCommentsService.Data;
using RatingCommentsService.Models;
using RatingCommentsService.Models.Dto;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
// Connection string
// ----------------------------------------------------------------
var conn =
    builder.Configuration.GetConnectionString("Postgres")
    ?? builder.Configuration["ConnectionStrings:Postgres"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
    ?? "Host=postgres;Port=5432;Database=dogs;Username=postgres;Password=postgres";

// ----------------------------------------------------------------
// EF Core + PostgreSQL
// ----------------------------------------------------------------
builder.Services.AddDbContext<AppDb>(opt =>
{
    opt.UseNpgsql(conn);
});

// JSON
builder.Services.AddControllers().AddJsonOptions(opt => { });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddHealthChecks();
var app = builder.Build();

// ----------------------------------------------------------------
// Auto migrations OR EnsureCreated
// ----------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    var disableEf = Environment.GetEnvironmentVariable("DOTNET_EF_DISABLED");

    try
    {
        if (string.IsNullOrEmpty(disableEf))
        {
            db.Database.Migrate();
            Console.WriteLine(">>> Migrations applied.");
        }
        else
        {
            db.Database.EnsureCreated();
            Console.WriteLine(">>> EnsureCreated executed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB init error: " + ex.Message);
    }
}

// ----------------------------------------------------------------
// Development tools
// ----------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----------------------------------------------------------------
// API endpoints
// ----------------------------------------------------------------
app.UseCors("allowAll");
app.MapHealthChecks("/health");
// POST /ratings/{dogId}
app.MapPost("/ratings/{dogId:long}", async (long dogId, CreateRatingDto dto, AppDb db) =>
{
    if (dto.Value < 1 || dto.Value > 5) return Results.BadRequest("Value must be 1..5");

    var r = new DogRating { DogId = dogId, Value = dto.Value };
    db.Ratings.Add(r);
    await db.SaveChangesAsync();

    return Results.Created($"/ratings/{r.Id}", r);
});

// GET /ratings/{dogId}
app.MapGet("/ratings/{dogId:long}", async (long dogId, AppDb db) =>
{
    var list = await db.Ratings
        .Where(x => x.DogId == dogId)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return Results.Ok(list);
});

// GET /ratings/{dogId}/avg
app.MapGet("/ratings/{dogId:long}/avg", async (long dogId, AppDb db) =>
{
    var avg = await db.Ratings
        .Where(x => x.DogId == dogId)
        .AverageAsync(x => (double?)x.Value);

    return Results.Ok(new { dogId, avg = avg ?? 0 });
});

// POST /comments/{dogId}
app.MapPost("/comments/{dogId:long}", async (long dogId, CreateCommentDto dto, AppDb db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Text))
        return Results.BadRequest("Text required");

    var c = new DogComment { DogId = dogId, Text = dto.Text };
    db.Comments.Add(c);
    await db.SaveChangesAsync();

    return Results.Created($"/comments/{c.Id}", c);
});

// GET /comments/{dogId}
app.MapGet("/comments/{dogId:long}", async (long dogId, AppDb db) =>
{
    var list = await db.Comments
        .Where(x => x.DogId == dogId)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return Results.Ok(list);
});

app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok());

// ----------------------------------------------------------------
app.Run();
