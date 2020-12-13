using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public ObservationsController(ApplicationDbContext context, IHubContext<UpdateHub> hub)
        {
            _context = context;
            _hubContext = hub;
        }

        // Gets 3 most recent observations
        [HttpGet("last3")]
        public async Task<ActionResult<IEnumerable<Observation>>> GetObservations()
        {
            var observations = await _context.Observations.OrderByDescending(o => o.Time).Take(3).ToListAsync();

            foreach (var o in observations)
            {
                o.AirPressure = Math.Round(o.AirPressure, 1);
                o.Temperature = Math.Round(o.Temperature, 1);
            }

            return observations;

        } 

        // get within time range
        [HttpGet("daterange/{start}/{end}")]
        public async Task<ActionResult<IEnumerable<Observation>>> GetObservations(DateTime start, DateTime end)
        {
            var observations = await _context.Observations.Where(o => o.Time >= start && o.Time <= end)
                .OrderByDescending(o => o.Time).ToListAsync();

            foreach (var o in observations)
            {
                o.AirPressure = Math.Round(o.AirPressure, 1);
                o.Temperature = Math.Round(o.Temperature, 1);
            }

            return observations;
        }

        
        // get by id
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Observation>> GetWeatherObservation(int id)
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

        // post observation
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Observation>> PostWeatherObservation(Observation o)
        {
            var newObservation = new Observation()
            {
                Time = o.Time,
                Humidity = o.Humidity,
                Temperature = o.Temperature,
                AirPressure = o.AirPressure,
                LocationName = o.LocationName,
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                Description = o.Description
            };

            _context.Observations.Add(newObservation);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("create", o);

            return CreatedAtAction("GetWeatherObservation", new { id = o.ObservationId }, newObservation);
        }
    }
}
