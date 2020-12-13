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
        private ObservationsController _uut;
        private IHubContext<UpdateHub> _fakeHub;

        private DbContextOptions<ApplicationDbContext> _opt;
        private ApplicationDbContext _context;

        private readonly DateTime _initialTime = new DateTime(2020, 01, 02);
        
        List<Observation> _dummyData = new List<Observation>(){
            new Observation()
            {
                ObservationId = 1,
                Time = new DateTime(2020, 01, 02)
            },
            new Observation()
            {
                ObservationId = 2,
                Time = new DateTime(2020, 01, 02).AddHours(2)
            },
            new Observation()
            {
                ObservationId = 3,
                Time = new DateTime(2020, 01, 02).AddHours(5)
            },
            new Observation()
            {
                ObservationId = 4,
                Time = new DateTime(2020, 01, 02).AddDays(1).AddHours(3)
            },
        };

    [SetUp]
        public void Setup()
        {
            _opt = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "TestDB").Options;
            _fakeHub = Substitute.For<IHubContext<UpdateHub>>();

            List<Observation> weatherObservations = _dummyData;
            _context = new ApplicationDbContext(_opt);
            _context.Database.EnsureCreated();
            _context.Observations.AddRange(weatherObservations);
            _context.SaveChanges();

            _uut = new ObservationsController(_context, _fakeHub);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [Test]
        public async Task GetObservations_ListCountIsCorrect()
        {
            var obs = await _uut.GetObservations();
            var obsList = obs.Value.ToList();

            Assert.AreEqual(obsList.Count, 3);
        }

        [Test]
        public async Task GetObservations_ValuesAreCorrect()
        {
            var obs = await _uut.GetObservations();
            var obsList = obs.Value.ToList();

            var obsExpected = GetDtoWeatherObservationsForTest().OrderByDescending(o => o.Time).Take(3).ToList();

            for(int i = 0; i < 3; i++)
            {
                Assert.That(obsList[i].Time, Is.EqualTo(obsExpected[i].Time));
            }
        }

        [Test]
        public async Task GetObservations_WithTimeSpanThreeHours_DbSeeded_ReturnsCorrectDtoWeatherObservations()
        {
            DateTime startTime = _initialTime;
            DateTime endTime = _initialTime.AddHours(3); //Should only contain 2 weatherObservations in this time-range

            var result = await _uut.GetObservations(startTime, endTime);
            var returnedList = result.Value.ToList();

            Assert.That(returnedList.Count, Is.EqualTo(2));
            Assert.That(returnedList[0].Time, Is.GreaterThanOrEqualTo(startTime).And.LessThanOrEqualTo(endTime));
            Assert.That(returnedList[1].Time, Is.GreaterThanOrEqualTo(startTime).And.LessThanOrEqualTo(endTime));
        }

        // HELPERS

        private List<Observation> GetDtoWeatherObservationsForTest()
        {
            var weatherObservations = _dummyData;

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
    }
}