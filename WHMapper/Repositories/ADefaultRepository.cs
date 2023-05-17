using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WHMapper.Repositories
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C">Server DbContext</typeparam>
    /// <typeparam name="T">Object to Manage in database</typeparam>
    /// <typeparam name="U">type of Id of Object</typeparam>
    public abstract class ADefaultRepository<C,T, U> : IDefaultRepository<T, U>
    {
        protected readonly C _dbContext;

        protected abstract Task<IEnumerable<T>?> AGetAll();
        protected abstract Task<T?> AGetById(U id);
        protected abstract Task<T?> ACreate(T item);
        protected abstract Task<T?> AUpdate(U id, T item);
        protected abstract Task<T?> ADeleteById(U id);


        protected static SemaphoreSlim semSlim = new SemaphoreSlim(1, 1);

        public ADefaultRepository(C dbContext)
        {
            _dbContext = dbContext; 
        }

        public async Task<T?> Create(T item)
        {

            return await ACreate(item);
        }

        public async Task<T?> DeleteById(U id)
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
