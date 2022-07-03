
// building configuration file 

using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();


var servicesConfig = config.GetSection("Services");
// Loading configuration values as variables 

var tempServiceConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempServiceConfig["Host"];
var tempServicePort = tempServiceConfig["Port"];

var precipServiceConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipServiceConfig["Host"];
var precipServicePort = precipServiceConfig["Port"];


// Creating a list of zip codes .. basically we are trying to mock some data 
var zipCodes = new List<string>
{
    "23444",
    "22345",
    "99759",
    "60348",
    "09755",
};

Console.WriteLine("Starting Data load");

// Creating separate http client for temprature service and precipitation service 

var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipitationHttpClient = new HttpClient();
precipitationHttpClient.BaseAddress = new Uri($"http://{precipServiceHost}:{precipServicePort}");


foreach(var zip in zipCodes)
{
    Console.WriteLine($"Processing zip code: {zip}");
    var from = DateTime.Now.AddYears(-2);
    var thru = DateTime.Now; 

    for(var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
    {
        var temps = PostTemp(zip, day, temperatureHttpClient);
        PostPrecip(temps[0], zip, day, precipitationHttpClient);
    }
}

void PostPrecip(int lowTemp, string zip, DateTime day, HttpClient precipitationHttpClient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1;
    PrecipitationModel precipitation;

    if (isPrecip)
    {
        var precipCm = rand.Next(1, 50);
        if(lowTemp < 0)
        {
            precipitation = new PrecipitationModel
            {
                AmountInCm = precipCm,
                WeatherType = "snow",
                ZipCode = zip,
                CreatedOn = day
            };
        }
        else
        {
            precipitation = new PrecipitationModel
            {
                AmountInCm = precipCm,
                WeatherType = "rain",
                ZipCode = zip,
                CreatedOn = day
            };
        }
    }
    else
    {
        precipitation = new PrecipitationModel
        {
            AmountInCm = 0,
            WeatherType = "none",
            ZipCode = zip,
            CreatedOn = day
        };
    }

    // lines above we are generating random weather condition and following up, going to post them 
    var precipResponse = precipitationHttpClient.PostAsJsonAsync("observation", precipitation).Result;

    // if post process is successful we are going to print some report 
    if (precipResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted precipitation: Date: {day:d}" + $"zip: {zip}" + $"Type: {precipitation.WeatherType}" + $"Amont (Cm.): {precipitation.AmountInCm}");
    }
}

List<int> PostTemp(string zip, DateTime day, HttpClient http)
{
    // generating high low temperature 
    var rand = new Random();
    var t1 = rand.Next(-20, 100); 
    var t2 = rand.Next(-20, 100);
    var HiLoTemps = new List<int> { t1, t2 };
    HiLoTemps.Sort();
    // using HiLoTemps list to generate temerature object 

    var temperatureObservation = new TemperatureModel
    {
        TempLowC = HiLoTemps[0],
        TempHighC = HiLoTemps[1],
        ZipCode = zip,
        CreatedOn = day
    };

    // And we are gpoing to post it. 
    var tempResponse = http.PostAsJsonAsync("observation", temperatureObservation).Result;

    if (tempResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Temperature: Date: {day:d}" + $"zip: {zip}" + $"Lo (C): {HiLoTemps[0]}" + $"Hi (C): {HiLoTemps[1]}");
    }
    else
    {
        Console.WriteLine(tempResponse.ToString());
    }
    return HiLoTemps;
}