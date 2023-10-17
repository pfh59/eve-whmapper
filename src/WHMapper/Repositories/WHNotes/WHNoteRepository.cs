using System;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;

namespace WHMapper.Repositories.WHNotes
{
	public class WHNoteRepository : ADefaultRepository<WHMapperContext, WHNote, int>, IWHNoteRepository
    {
        public WHNoteRepository(ILogger<WHNoteRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }

        protected override async Task<WHNote?> ACreate(WHNote item)
        {

            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHNotes.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WhNote, Solar System Id  : {0}", item.SoloarSystemId));
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                int deleteRow = await context.DbWHNotes.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHNote>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHNotes.ToListAsync();
            }
        }

        protected override async Task<WHNote?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHNotes.SingleOrDefaultAsync(x => x.Id == id);
            }

        }

        protected override async Task<WHNote?> AUpdate(int id, WHNote item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHNotes.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to udpate WHNote, Solar System Id : {0}", item.SoloarSystemId));
                    return null;
                }
            }
        }

        public async Task<WHNote?> GetBySolarSystemId(int solardSystemId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHNotes.SingleOrDefaultAsync(x => x.SoloarSystemId == solardSystemId);
            }
        }
    }
}

