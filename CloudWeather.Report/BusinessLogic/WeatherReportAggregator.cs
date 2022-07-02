using CloudWeather.Report.Configs;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CloudWeather.Report.BusinessLogic;

/// <summary>
/// Aggregates data from multiple external sources to build a weather report 
/// </summary>

// for the sake of simplicty we add our interface here 
public interface IWeatherReportAggrigator
{
    ///<summary>
    ///Builds and returnes a weekly weather report 
    ///Presists weekly weather report data 
    ///</summary>
    public Task<WeatherReport> BuildReport(string zip, int days);
}
public class WeatherReportAggregator :IWeatherReportAggrigator
{
    private readonly IHttpClientFactory _http; 
    private readonly ILogger<WeatherReportAggregator> _logger;
    private readonly WeatherDataConfig _weatherDataConfig;
    private readonly WeatherReportDbContext _db;

    // constructor 
    public WeatherReportAggregator(IHttpClientFactory http,
                                   ILogger<WeatherReportAggregator> logger,
                                   IOptions<WeatherDataConfig> weatherConfig,
                                       WeatherReportDbContext db)
        {
            _http = http; 
            _logger = logger;
            _weatherDataConfig = weatherConfig.Value; 
            _db = db;
        }

    public async Task<WeatherReport> BuildReport(string zip, int days)
    {
        var httpClient = _http.CreateClient();

        var precipData = await FetchPrecipationData(httpClient, zip, days);
        var totalSnow = GetTotalSnow(precipData);
        var totalRain = GetTotalRain(precipData);

        // Loggeing the data 
        _logger.LogInformation($"zip:{zip} over last {days} days " + $"total snow: {totalSnow}, rain {totalRain}");


        // -------------------------------------------- // 

        var tempData = await FetchTempratureData(httpClient, zip, days);
        var avarageHighTemp = tempData.Average(t => t.TempHignC);
        var avarageLowTemp = tempData.Average(t => t.TempLowC);

        // Loggeing the data 
        _logger.LogInformation($"zip: {zip} over last {days} days" + $"Lo temp: {avarageLowTemp}, hi temp {avarageHighTemp}");

        // creating actual report 
        var weatherReport = new WeatherReport
        {
            AvarageHighC = Math.Round(avarageHighTemp, 1),
            AvarageLowC = Math.Round(avarageLowTemp, 1),
            RainFallTotalCm = totalRain,
            SnowTotalCm = totalSnow,
            ZipCode = zip,
            CreatedOn = DateTime.UtcNow,
        };

        // TODO: Use chached weather reports instead of making round trips when possible.
        // saving the result in our database 
        _db.Add(weatherReport);
        await _db.SaveChangesAsync();

        return weatherReport; 
    }

    private static decimal GetTotalSnow(IEnumerable<PrecipitationModel> precipdata)
    {
        var totalSnow = precipdata.Where(precip => precip.WeatherType == "snow").Sum(precip => precip.AmountCm);

        return Math.Round(totalSnow, 1);
    }

    private static decimal GetTotalRain(IEnumerable<PrecipitationModel> precipdata)
    {
        var totalSnow = precipdata.Where(precip => precip.WeatherType == "rain").Sum(precip => precip.AmountCm);

        return Math.Round(totalSnow, 1);
    }



    private async Task<List<TemperatureModel>> FetchTempratureData(HttpClient httpClient, string zip, int days)
    {
        var endpoint = BuildTempratureServiceEndPoint(zip, days); 
        var temperatureRecords = await httpClient.GetAsync(endpoint);

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var temperatureData = await temperatureRecords.Content.ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptions);

        return temperatureData ?? new List<TemperatureModel>(); 
    }

    private async Task<List<PrecipitationModel>> FetchPrecipationData(HttpClient httpClient, string zip, int days)
    {
        var endpoint = BuildPrecipitationEndPoint(zip, days); 
        var precipRecords = await httpClient.GetAsync(endpoint);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var precipData = await precipRecords.Content.ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializerOptions);

        return precipData ?? new List<PrecipitationModel>();    
        
    }

    private string BuildPrecipitationEndPoint(string zip, int days)
    {
        var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol; 
        var precipServiceHost = _weatherDataConfig.PrecipDataHost;
        var precipServicePort = _weatherDataConfig.PrecipDataPort;
        return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
    }

    // Helper method 
    private string BuildTempratureServiceEndPoint(string zip, int days)
    {
        var tempServiceProtocol = _weatherDataConfig.TempDataProtocol; 
        var tempServiceHost = _weatherDataConfig.TempDataHost;
        var tempServicePort = _weatherDataConfig.TempDataPosrt;
        return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
    }


}
