using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        // list to keep all customers in memory
        private static List<Customer> customers = new List<Customer>();

        // GET api/Customer - get all customers
        [HttpGet]
        public ActionResult<List<Customer>> GetCustomers()
        {
            return Ok(customers);
        }

        // GET api/Customer/{id} - get one customer by id, used by Rental Service
        [HttpGet("{id}")]
        public ActionResult<Customer> GetCustomerById(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return NotFound("Customer not found!");
            return Ok(customer);
        }

        // POST api/Customer - register new customer
        [HttpPost]
        public ActionResult AddCustomer(Customer customer)
        {
            customer.Id = customers.Count + 1;
            customers.Add(customer);
            return Ok("Customer added successfully!");
        }

        // DELETE api/Customer/{id} - remove customer by id
        [HttpDelete("{id}")]
        public ActionResult DeleteCustomer(int id)
        {
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return NotFound("Customer not found!");
            customers.Remove(customer);
            return Ok("Customer deleted successfully!");
        }
    }
}
