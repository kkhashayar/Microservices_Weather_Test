using CloudWeather.Report.BusinessLogic;
using CloudWeather.Report.Configs;
using CloudWeather.Report.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dp injection, injecting weather report aggregrator service 
builder.Services.AddHttpClient();
builder.Services.AddTransient<IWeatherReportAggrigator, WeatherReportAggregator>();
builder.Services.AddOptions();
builder.Services.Configure<WeatherDataConfig>(builder.Configuration.GetSection("WeatherDataConfig")); 


// Injecting db context 
builder.Services.AddDbContext<WeatherReportDbContext>(
    options =>
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
    }, ServiceLifetime.Transient);

var app = builder.Build();


//Creating get request 
app.MapGet("/weather-report/{zip}", async (string zip, [FromQuery] int? days, IWeatherReportAggrigator weatherAgg) => {

    if (days is null || days > 30 || days < 1)
    {
        return Results.BadRequest("Query must be in range 1 - 30 "); 
        
    }

    var report = await weatherAgg.BuildReport(zip, days.Value);
    return Results.Ok(report); 
});



app.Run();
