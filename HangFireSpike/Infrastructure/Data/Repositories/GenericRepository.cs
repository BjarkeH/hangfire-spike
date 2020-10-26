using Hangfire;
using HangFireSpike.Core.Models;
using HangFireSpike.Infrastructure.Caching;
using HangFireSpike.Infrastructure.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HangFireSpike.Infrastructure.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        //private readonly string cacheKey = $"{typeof(T)}";
        private readonly ApplicationDbContext _dbContext;
        private readonly Func<CacheTech, ICacheService> _cacheService;
        public GenericRepository(ApplicationDbContext applicationDbContext, Func<CacheTech, ICacheService> cacheService)
        {
            _dbContext = applicationDbContext;
            _cacheService = cacheService;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue(() => RefreshCacheListResult());            
            return entity;

        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();            

            var jobId = BackgroundJob.Enqueue(() => RefreshCacheListResult());
            BackgroundJob.ContinueJobWith(jobId, () => Debug.WriteLine("\n\n FINISHES \n\n"));
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            var cacheKey = GetCacheKey();

            if (!_cacheService(cacheTech).TryGet(cacheKey, out IReadOnlyList<T> cachedList))
            {
                cachedList = await _dbContext.Set<T>().AsNoTracking().ToListAsync();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);

            //var cacheKey = GetCacheKey();

            //if(!_cacheService(cacheTech).TryGet(cacheKey, out T entity))
            //{
            //    entity = await _dbContext.Set<T>().FindAsync(id);
            //    _cacheService(cacheTech).Set(cacheKey, entity);
            //}
            //return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue(() => RefreshCacheListResult());            
        }        


        //public async Task RefreshCacheEntityResult(int id)
        //{
        //    var cacheKey = GetCacheKey<T>(id);

        //    _cacheService(cacheTech).Remove(cacheKey);
        //    var cachedResult = await _dbContext.Set<T>().FindAsync(id);
        //    _cacheService(cacheTech).Set(cacheKey, cachedResult);
        //}

        public async Task RefreshCacheListResult()
        {
            var cacheKey = GetCacheKey();

            _cacheService(cacheTech).Remove(cacheKey);
            var cachedList = await _dbContext.Set<T>().ToListAsync();
            _cacheService(cacheTech).Set(cacheKey, cachedList);
        }

        //public async Task RemoveFromCache(int id)
        //{
        //    var cacheKey = GetCacheKey<T>(id);
        //    await Task.Run(() => 
        //    { 
        //        _cacheService(cacheTech).Remove(cacheKey); 
        //    });            
        //}

        private string GetCacheKey() 
        {
            return $"{typeof(T)}";
        }
    }
}
