using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RentalService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        // list to store all rental records
        private static List<Rental> rentals = new List<Rental>();

        // http client to call other services
        private readonly HttpClient _httpClient = new HttpClient();

        // addresses of other services
        private const string VehicleApiUrl = "https://localhost:7201";
        private const string CustomerApiUrl = "https://localhost:7202";

        // GET api/Rental - get all rental records
        [HttpGet]
        public ActionResult<List<Rental>> GetRentals()
        {
            return Ok(rentals);
        }

        // POST api/Rental/rent - rent a vehicle
        // checks that customer exists and vehicle is available before renting
        [HttpPost("rent")]
        public async Task<ActionResult> RentVehicle([FromBody] RentRequest request)
        {
            // check if customer exists in Customer Service
            var customerResponse = await _httpClient.GetAsync($"{CustomerApiUrl}/api/Customer/{request.CustomerId}");
            if (!customerResponse.IsSuccessStatusCode)
                return BadRequest("Customer not found!");

            // check if vehicle exists in Vehicle Service
            var vehicleResponse = await _httpClient.GetAsync($"{VehicleApiUrl}/api/Vehicle/{request.VehicleId}");
            if (!vehicleResponse.IsSuccessStatusCode)
                return BadRequest("Vehicle not found!");

            // check if vehicle is available
            var vehicleJson = await vehicleResponse.Content.ReadAsStringAsync();
            var vehicle = JsonSerializer.Deserialize<VehicleDto>(vehicleJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (vehicle == null || !vehicle.IsAvailable)
                return BadRequest("Vehicle is not available!");

            // mark vehicle as rented in Vehicle Service
            var content = new StringContent("false", System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"{VehicleApiUrl}/api/Vehicle/{request.VehicleId}/availability", content);

            // save rental record
            var rental = new Rental
            {
                Id = rentals.Count + 1,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                RentalDate = DateTime.Now
            };
            rentals.Add(rental);

            return Ok($"Vehicle {request.VehicleId} rented to customer {request.CustomerId} successfully!");
        }

        // PUT api/Rental/return/{rentalId} - return a vehicle
        [HttpPut("return/{rentalId}")]
        public async Task<ActionResult> ReturnVehicle(int rentalId)
        {
            // find the rental record
            var rental = rentals.FirstOrDefault(r => r.Id == rentalId);
            if (rental == null) return NotFound("Rental record not found!");
            if (rental.ReturnDate != null) return BadRequest("Vehicle already returned!");

            // set return date
            rental.ReturnDate = DateTime.Now;

            // mark vehicle as available again in Vehicle Service
            var content = new StringContent("true", System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"{VehicleApiUrl}/api/Vehicle/{rental.VehicleId}/availability", content);

            return Ok($"Vehicle returned successfully! Rental duration: {(rental.ReturnDate - rental.RentalDate)?.TotalHours:F1} hours");
        }
    }

    // helper class for rent request body
    public class RentRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
    }

    // helper class to read vehicle data from Vehicle Service
    public class VehicleDto
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
    }
}
