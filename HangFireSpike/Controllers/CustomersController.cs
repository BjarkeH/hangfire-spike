using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HangFireSpike.Core.Models;
using HangFireSpike.Infrastructure;
using HangFireSpike.Infrastructure.Data.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HangFireSpike.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _repository;
        private readonly ApplicationDbContext _applicationDbContext;
        public CustomersController(ICustomerRepository repository, ApplicationDbContext ctx)
        {
            _repository = repository;
            _applicationDbContext = ctx;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer is null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            await _repository.UpdateAsync(customer);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Post(Customer customer)
        {
            await _repository.AddAsync(customer);
            return CreatedAtAction("Get", new { id = customer.Id }, customer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer is null)
                return NotFound();

            await _repository.DeleteAsync(customer);
            return Ok(customer);
        }

        [HttpOptions]
        public async Task<IActionResult> Test()
        {
            var customers = new List<Customer>();

            for (int i = 0; i < 15000; i++)
            {
                customers.Add(new Customer
                {
                    Contact = Guid.NewGuid().ToString(),
                    DateOfBirth = DateTime.Now,
                    Email = Guid.NewGuid().ToString(),
                    FirstName = Guid.NewGuid().ToString(),
                    LastName = Guid.NewGuid().ToString()
                });
            }

            using (var ctx = _applicationDbContext)
            {
                await ctx.Customers.AddRangeAsync(customers);

                var jobId = Hangfire.BackgroundJob.Enqueue(() => _repository.RefreshCacheListResult());
                BackgroundJob.ContinueJobWith(jobId,() => Debug.WriteLine("\n\n FINISHES \n\n"));

                return Ok(await ctx.SaveChangesAsync());
            }

            
        }
    }
}
