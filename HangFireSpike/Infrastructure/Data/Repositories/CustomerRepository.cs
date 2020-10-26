using HangFireSpike.Core.Models;
using HangFireSpike.Infrastructure.Caching;
using HangFireSpike.Infrastructure.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFireSpike.Infrastructure.Data.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly DbSet<Customer> _customer;
        public CustomerRepository(ApplicationDbContext applicationDbContext, Func<CacheTech, ICacheService> cacheService) : base(applicationDbContext, cacheService)
        {
            _customer = applicationDbContext.Set<Customer>();
        }
    }
}
