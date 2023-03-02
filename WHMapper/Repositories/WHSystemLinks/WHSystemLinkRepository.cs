using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Repositories.WHSystemLinks
{
    public class WHSystemLinkRepository : ADefaultRepository<WHMapperContext, WHSystemLink, int>, IWHSystemLinkRepository
	{
        public WHSystemLinkRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHSystemLink?> ACreate(WHSystemLink item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHSystemLinks.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHSystemLink?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                _dbContext.DbWHSystemLinks.Remove(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<IEnumerable<WHSystemLink>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHSystemLinks.ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHSystemLink?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHSystemLinks.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHSystemLink?> AUpdate(int id, WHSystemLink item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext?.DbWHSystemLinks?.Update(item);
                await _dbContext.SaveChangesAsync();
                return item;
            }
            finally
            {
                semSlim.Release();
            }
        }
    }
}

