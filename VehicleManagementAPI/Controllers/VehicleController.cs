using Microsoft.AspNetCore.Mvc;

namespace VehicleManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        // list to store all vehicles in memory
        private static List<Vehicle> vehicles = new List<Vehicle>();

        // GET api/Vehicle - returns only available vehicles
        [HttpGet]
        public ActionResult<List<Vehicle>> GetAvailableVehicles()
        {
            var available = vehicles.Where(v => v.IsAvailable).ToList();
            return Ok(available);
        }

        // GET api/Vehicle/all - returns all vehicles including rented ones
        [HttpGet("all")]
        public ActionResult<List<Vehicle>> GetAllVehicles()
        {
            return Ok(vehicles);
        }

        // POST api/Vehicle - add new vehicle to the list
        [HttpPost]
        public ActionResult AddVehicle(Vehicle vehicle)
        {
            vehicle.Id = vehicles.Count + 1;
            vehicle.IsAvailable = true; // new vehicle is available by default
            vehicles.Add(vehicle);
            return Ok("Vehicle added successfully!");
        }

        // PUT api/Vehicle/{id}/availability - update if vehicle is available or not
        [HttpPut("{id}/availability")]
        public ActionResult UpdateAvailability(int id, [FromBody] bool isAvailable)
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null) return NotFound("Vehicle not found!");

            vehicle.IsAvailable = isAvailable;
            return Ok($"Vehicle {id} availability updated to {isAvailable}");
        }

        // GET api/Vehicle/{id} - get one vehicle by id, used by Rental Service
        [HttpGet("{id}")]
        public ActionResult<Vehicle> GetVehicleById(int id)
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null) return NotFound("Vehicle not found!");
            return Ok(vehicle);
        }
    }
}
