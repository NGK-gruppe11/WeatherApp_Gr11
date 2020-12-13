using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using WeatherApp.Controllers;
using WeatherApp.Data;
using WeatherApp.Hubs;
using WeatherApp.Models;

namespace WeatherStationWebApp.Tests
{
    [TestFixture]
    public class WeatherObservationControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private IHubContext<UpdateHub> _mockHub;
        private ObservationsController _uut;
        private readonly DateTime _initialTime = new DateTime(2020, 10, 10, 10, 10, 10);

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase").Options;

            _mockHub = Substitute.For<IHubContext<UpdateHub>>();

            List<Observation> weatherObservations = GetWeathersObservationsForTest();

            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureCreated();

                context.Observations.AddRange(weatherObservations);
                context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Test]
        public async Task GetObservations_DbSeeded_ReturnsDtoWeatherObservationsWithCorrectDates()
        {
            // Arrange
            await using (var context = new ApplicationDbContext(_options))
            {
                _uut = new ObservationsController(context, _mockHub);

                // Act
                var result = await _uut.GetObservations();
                var returnedList = result.Value.ToList();

                // Assert
                var expectedDtoWeatherObservations =
                    GetDtoWeatherObservationsForTest().OrderByDescending(o => o.Time).Take(3).ToList();

                Assert.Multiple((() =>
                {
                    Assert.That(returnedList.Count, Is.EqualTo(3));

                    Assert.That(returnedList[0].Time, Is.EqualTo(expectedDtoWeatherObservations[0].Time));
                    Assert.That(returnedList[1].Time, Is.EqualTo(expectedDtoWeatherObservations[1].Time));
                    Assert.That(returnedList[2].Time, Is.EqualTo(expectedDtoWeatherObservations[2].Time));
                }));

            }
        }

        //[Test]
        //public async Task GetObservations_WithDateInitialTimePlus1Day_DbSeeded_ReturnsOneDtoWeatherObservationWithCorrectDate()
        //{
        //    // Arrange

        //    DateTime date = _initialTime.AddDays(1).Date; // Test data should only contain one observation with this date

        //    await using (var context = new ApplicationDbContext(_options))
        //    {
        //        _uut = new ObservationsController(context, _mockHub);

        //        // Act
        //        var result = await _uut.GetObservations(date);
        //        var returnedList = result.Value.ToList();

        //        // Assert
        //        Assert.Multiple((() =>
        //        {
        //            Assert.That(returnedList.Count, Is.EqualTo(1));
        //            Assert.That(returnedList[0].Date.Date, Is.EqualTo(date));
        //        }));

        //    }
        //}

        [Test]
        public async Task GetObservations_WithTimeSpanThreeHours_DbSeeded_ReturnsCorrectDtoWeatherObservations()
        {
            // Arrange
            DateTime startTime = _initialTime;
            DateTime endTime = _initialTime.AddHours(3); //Should only contain 2 weatherObservations in this time-range

            await using (var context = new ApplicationDbContext(_options))
            {
                _uut = new ObservationsController(context, _mockHub);

                // Act
                var result = await _uut.GetObservations(startTime, endTime);
                var returnedList = result.Value.ToList();

                // Assert
                Assert.Multiple((() =>
                {
                    Assert.That(returnedList.Count, Is.EqualTo(2));
                    Assert.That(returnedList[0].Time, Is.GreaterThanOrEqualTo(startTime).And.LessThanOrEqualTo(endTime));
                    Assert.That(returnedList[1].Time, Is.GreaterThanOrEqualTo(startTime).And.LessThanOrEqualTo(endTime));
                }));

            }
        }

        // HELPERS

        private List<Observation> GetDtoWeatherObservationsForTest()
        {
            var weatherObservations = GetWeathersObservationsForTest();

            List<Observation> dtoWeatherObservations = new List<Observation>();

            foreach (var weatherObservation in weatherObservations)
            {
                dtoWeatherObservations.Add(new Observation()
                {
                    Time = weatherObservation.Time,
                    Temperature = weatherObservation.Temperature,
                    Humidity = weatherObservation.Humidity,
                    AirPressure = weatherObservation.AirPressure,
                    Latitude = weatherObservation.Latitude,
                    Longitude = weatherObservation.Longitude,
                    LocationName = weatherObservation.LocationName
                });
            }

            return dtoWeatherObservations;
        }

        private List<Observation> GetWeathersObservationsForTest()
        {

            return new List<Observation>()
            {
                new Observation()
                {
                    ObservationId = 1,
                    Time = _initialTime,
                    Temperature = 20.5,
                    Humidity = 2,
                    AirPressure = 1.4,
                    Latitude = 20,
                    Longitude = 20,
                    LocationName = "Valhalla"
                },
                new Observation()
                {
                    ObservationId = 2,
                    Time = _initialTime.AddHours(2),
                    Temperature = 21.5,
                    Humidity = 3,
                    AirPressure = 1.4,
                    Latitude = 40,
                    Longitude = 40,
                    LocationName = "Down under"
                },
                new Observation()
                {
                    ObservationId = 3,
                    Time = _initialTime.AddHours(5),
                    Temperature = 20.5,
                    Humidity = 2,
                    AirPressure = 1.4,
                    Latitude = 20,
                    Longitude = 20,
                    LocationName = "Valhalla"
                },
                new Observation()
                {
                    ObservationId = 4,
                    Time = _initialTime.AddDays(1).AddHours(3),
                    Temperature = 16.3,
                    Humidity = 5,
                    AirPressure = 1.1,
                    Latitude = 40,
                    Longitude = 40,
                    LocationName = "Down under"
                },
            };
        }
    }
}