using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WHMapper.Repositories
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C">Server DbContext</typeparam>
    /// <typeparam name="T">Object to Manage in database</typeparam>
    /// <typeparam name="U">type of Id of Object</typeparam>
    public abstract class ADefaultRepository<C,T, U>  where C : DbContext 
    {
        protected readonly ILogger _logger;
        protected readonly IDbContextFactory<C> _contextFactory;

        protected abstract Task<IEnumerable<T>?> AGetAll();
        protected abstract Task<T?> AGetById(U id);
        protected abstract Task<T?> ACreate(T item);
        protected abstract Task<T?> AUpdate(U id, T item);
        protected abstract Task<bool> ADeleteById(U id);

        protected ADefaultRepository(ILogger logger,IDbContextFactory<C> dbContext)
        {
            _logger = logger;
            _contextFactory = dbContext;

        }

        public async Task<T?> Create(T item)
        {

            return await ACreate(item);
        }

        public async Task<bool> DeleteById(U id)
        {
            return await ADeleteById(id);
        }

        public async Task<IEnumerable<T>?> GetAll()
        {
            return await AGetAll();
        }

        public async Task<T?> GetById(U id)
        {
            return await AGetById(id);
        }

        public async Task<T?> Update(U id, T item)
        {
            return await AUpdate(id, item);
        }

        

    }
}
