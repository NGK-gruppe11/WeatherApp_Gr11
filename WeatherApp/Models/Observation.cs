using System;
using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Models
{
    public class Observation
    {
        [Key]
        public int ObservationId { get; set; }
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double AirPressure { get; set; }
        public string Description { get; set; }

        // Location
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}