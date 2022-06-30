using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Sending parcipitation data to user when they request it 
// Users provide zip code and time using formquery, query values 
var app = builder.Build();

// simple way of routing. using delicate for dynamic routing 
app.MapGet("/obzervation/{zip}", (string zip, [FromQuery] int? days) =>
{
   return Results.Ok(zip);
});


app.Run();
