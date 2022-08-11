using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSystems
{
    public class WHSystemRepository : ADefaultRepository<WHMapperContext,WHSystem,int>, IWHSystemRepository
    {
        public WHSystemRepository(WHMapperContext context) : base (context)
        {
        }

        protected override async Task<WHSystem?> ACreate(WHSystem item)
        {
            try
            {
                await _dbContext.DbWHSystems.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        protected override async Task<WHSystem?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.DbWHSystems.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHSystem>?> AGetAll()
        {
            
            if(_dbContext.DbWHSystems.Count()==0)
                return await _dbContext.DbWHSystems.ToListAsync();
            else
                return await _dbContext.DbWHSystems.OrderBy(x => x.Name).ToListAsync();
        }

        protected override async Task<WHSystem?> AGetById(int id)
        {
            return await _dbContext.DbWHSystems
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override async Task<WHSystem?> AUpdate(int id, WHSystem item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task<WHSystem?> GetByName(string name)
        {
            return await _dbContext.DbWHSystems.FirstOrDefaultAsync(x => x.Name == name);
        }

    }
}

