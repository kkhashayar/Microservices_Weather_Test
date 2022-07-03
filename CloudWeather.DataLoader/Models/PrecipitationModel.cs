﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudWeather.DataLoader.Models;

internal class PrecipitationModel
{
    public DateTime CreatedOn { get; set; }
    public decimal AmountInCm { get; set; }
    public string WeatherType { get; set; }
    public string ZipCode  { get; set; } 

}
