using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using WeatherApp.Models;
using WeatherApp.Controllers;
using WeatherApp.Data;
using WeatherApp.Hubs;

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

        List<Observation> _dummyData = new List<Observation>(){};

    [SetUp]
        public void Setup()
        {
            _opt = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "TestDB").Options;
            _fakeHub = Substitute.For<IHubContext<UpdateHub>>();

            //create 10 dummy observations. increment day each time
            for(int i = 1; i<=10; i++)
            {
                _dummyData.Add(
                    new Observation()
                    {
                        ObservationId = i,
                        Time = _initialTime.AddDays(i)
                    }
                );
            }
            
            _context = new ApplicationDbContext(_opt);
            _context.Database.EnsureCreated();
            _context.Observations.AddRange(_dummyData);
            _context.SaveChanges();

            _uut = new ObservationsController(_context, _fakeHub);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _dummyData.Clear();
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

            Assert.AreEqual(obsList[0].Time, _initialTime.AddDays(10));
            Assert.AreEqual(obsList[1].Time, _initialTime.AddDays(9));
            Assert.AreEqual(obsList[2].Time, _initialTime.AddDays(8));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public async Task GetObservations_Range_CorrectResults(int dayRange)
        {
            DateTime start = _initialTime;
            DateTime end = _initialTime.AddDays(dayRange);

            var obs = await _uut.GetObservationsRange(start, end);
            var obsList = obs.Value.ToList();

            Assert.AreEqual(obsList.Count, dayRange);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public async Task GetWeatherObservation_PassId_ReturnCorrectObservation(int idResult)
        {
            var obs = await _uut.GetObservationById(idResult);
            var obsVal = obs.Value;

            Assert.AreEqual(obsVal.ObservationId, idResult);
            Assert.AreEqual(obsVal.Time, _initialTime.AddDays(idResult));
        }
        
        [Test]
        public async Task PostWeatherObservation_ValueIsAppended()
        {
            await _uut.PostObservation(new Observation()
            {
                ObservationId = 11,
                Time = _initialTime.AddDays(11),
                Temperature = 20,
                AirPressure = 30,
                Humidity = 40,
                LocationName = "Aarhus",
                Latitude = 50,
                Longitude = 60,
                Description = "Nice and warm"
            });

            var obs = await _uut.GetObservationById(11);
            var obsVal = obs.Value;

            Assert.AreEqual(obsVal.ObservationId, 11);
            Assert.AreEqual(obsVal.Time, _initialTime.AddDays(11));
            Assert.AreEqual(obsVal.Temperature, 20);
            Assert.AreEqual(obsVal.AirPressure, 30);
            Assert.AreEqual(obsVal.Humidity, 40);
            Assert.AreEqual(obsVal.LocationName, "Aarhus");
            Assert.AreEqual(obsVal.Latitude, 50);
            Assert.AreEqual(obsVal.Longitude, 60);
            Assert.AreEqual(obsVal.Description, "Nice and warm");
        }
    }
}