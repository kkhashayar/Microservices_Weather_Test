using CloudWeather.Temprature.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Injecting db context 
builder.Services.AddDbContext<TempratureDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient);


var app = builder.Build();

// Get Request  
// simple way of routing. using delicate for dynamic routing 
app.MapGet("/observation/{zip}", async (string zip, [FromQuery] int? days, TempratureDbContext db) =>
{
    // Simple data check - filtering 
    if (days is null || days < 1 || days > 30)
    {
        return Results.BadRequest("Provide a 'days' query parameter between 1 and 30");
    }
    var startDate = DateTime.UtcNow - TimeSpan.FromDays(days.Value);
    var results = await db.Temprature.Where(p => p.ZipCode == zip && p.CreatedOn > startDate).ToListAsync();

    return Results.Ok(results);
});

app.MapPost("/observation", async (Temprature temprature, TempratureDbContext db) =>
{
    temprature.CreatedOn = temprature.CreatedOn.ToUniversalTime(); // some sort of mock data for now 
    await db.AddAsync(temprature);
    await db.SaveChangesAsync();

});

app.Run();
