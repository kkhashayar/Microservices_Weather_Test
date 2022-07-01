namespace CloudWeather.Report.DataAccess
{
    public class WeatherReport
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal AvarageHighC { get; set; }
        public decimal AvarageLowC { get; set; }
        public decimal RainFallTotalCm { get; set; }
        public decimal SnowTotalCm { get; set; }
        public string  ZipCode { get; set; }

    }
}
