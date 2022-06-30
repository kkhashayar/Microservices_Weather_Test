using CloudWeather.Precipitation.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Sending parcipitation data to user when they request it 
// Users provide zip code and time using formquery, query values 


// Injecting db context 
builder.Services.AddDbContext<PrecipDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient);

 

var app = builder.Build();

// simple way of routing. using delicate for dynamic routing 
app.MapGet("/obzervation/{zip}", async (string zip, [FromQuery] int? days, PrecipDbContext db) =>
{
    // Simple data check - filtering 
    if(days is null || days < 1 || days > 30 )
    {
        return Results.BadRequest("Provide a 'days' query parameter between 1 and 30");
    }
    var startDate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.Precipitation.Where(p => p.ZipCode == zip && p.CreatedOn > startDate).ToListAsync();

    return Results.Ok(results);
});


app.Run();
