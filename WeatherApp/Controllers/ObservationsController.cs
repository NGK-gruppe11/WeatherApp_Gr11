using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Hubs;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<UpdateHub> _hubContext;

        public ObservationsController(ApplicationDbContext context, IHubContext<UpdateHub> hub)
        {
            _context = context;
            _hubContext = hub;
        }

        // GET: api/WeatherObservations
        // Gets last 3 observations
        [HttpGet("last3")]
        public async Task<ActionResult<IEnumerable<Observation>>> GetObservations()
        {
            var weatherObservations = await _context.Observations.OrderByDescending(o => o.Time).Take(3).ToListAsync();

            foreach (var weatherObservation in weatherObservations)
            {
                weatherObservation.AirPressure = Math.Round(weatherObservation.AirPressure, 1);
                weatherObservation.Temperature = Math.Round(weatherObservation.Temperature, 1);
            }

            return weatherObservations;

        } 

        // GET: api/WeatherObservations/{date}
        [HttpGet("{startTime}/{endTime}")]
        public async Task<ActionResult<IEnumerable<Observation>>> GetObservations(DateTime startTime, DateTime endTime)
        {
            var observations = await _context.Observations.Where(o => o.Time >= startTime && o.Time <= endTime)
                .OrderByDescending(o => o.Time).ToListAsync();

            foreach (var o in observations)
            {
                o.AirPressure = Math.Round(o.AirPressure, 1);
                o.Temperature = Math.Round(o.Temperature, 1);
            }

            return observations;
        }

        
        // GET: api/WeatherObservations/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Observation>> GetWeatherObservation(long id)
        {
            var observations = await _context.Observations.FindAsync(id);

            if (observations == null)
            {
                return NotFound();
            }

            observations.AirPressure = Math.Round(observations.AirPressure, 1);
            observations.Temperature = Math.Round(observations.Temperature, 1);

            return observations;
        }

        // POST: api/WeatherObservations
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Observation>> PostWeatherObservation(Observation observation)
        {
            var newObservation = new Observation()
            {
                Time = observation.Time,
                Humidity = observation.Humidity,
                Temperature = observation.Temperature,
                AirPressure = observation.AirPressure,
                LocationName = observation.LocationName,
                Latitude = observation.Latitude,
                Longitude = observation.Longitude,
                Description = observation.Description
            };

            _context.Observations.Add(newObservation);
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("NewData", observation);

            return CreatedAtAction("GetWeatherObservation", new { id = observation.ObservationId }, newObservation);
        }

        // DELETE: api/WeatherObservations/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Observation>> DeleteWeatherObservation(long id)
        {
            var weatherObservation = await _context.Observations.FindAsync(id);
            if (weatherObservation == null)
            {
                return NotFound();
            }

            _context.Observations.Remove(weatherObservation);
            await _context.SaveChangesAsync();

            return weatherObservation;
        }

        private bool WeatherObservationExists(long id)
        {
            return _context.Observations.Any(e => e.ObservationId == id);
        }
    }
}
