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
        }

        protected override async Task<WHSystemLink?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.DbWHSystemLinks.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHSystemLink>?> AGetAll()
        {
            return await _dbContext.DbWHSystemLinks.ToListAsync();
        }

        protected override async Task<WHSystemLink?> AGetById(int id)
        {
            return await _dbContext.DbWHSystemLinks.FindAsync(id);
        }

        protected override async Task<WHSystemLink?> AUpdate(int id, WHSystemLink item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }
    }
}

