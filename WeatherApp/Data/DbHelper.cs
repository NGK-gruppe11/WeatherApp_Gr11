using System;
using WeatherApp.Models;

namespace WeatherApp.Data
{
    public class DbHelper
    {
        public static void DummyData(ApplicationDbContext context)
        {
            for (int i = 0; i<10; i++)
            {
                context.Observations.Add(new Observation()
                {
                    Time = DateTime.Now,
                    Temperature = 11.11,
                    Humidity = 22.22,
                    AirPressure = 33.33,
                    Description = "Interesting observation",
                    LocationName = "Aarhus",
                    Latitude = 44.44,
                    Longitude = 55.55
                });
            }

            context.SaveChanges();
        }
    }
}