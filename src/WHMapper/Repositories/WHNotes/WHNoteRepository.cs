using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

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

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHNotes.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WhNote, Solar System Id  : {SoloarSystemId}", item.SoloarSystemId);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
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
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHNotes.ToListAsync();
            }
        }

        protected override async Task<WHNote?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHNotes.SingleOrDefaultAsync(x => x.Id == id);
            }

        }

        protected override async Task<WHNote?> AUpdate(int id, WHNote item)
        {
            if (item == null)
            {
                _logger.LogError("Impossible to update WHNote, item is null");
                return null;
            }

            if (id != item.Id)
            {
                _logger.LogError("Impossible to update WHNote, id is different from item.Id");
                return null;
            }

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    context.DbWHNotes.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to udpate WHNote, Solar System Id : {SoloarSystemId}", item.SoloarSystemId);
                    return null;
                }
            }
        }

        protected override async Task<int> AGetCountAsync()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHNotes.CountAsync();
            }
        }

        public async Task<WHNote?> Get(int mapid, int solardSystemId)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHNotes.SingleOrDefaultAsync(x => x.SoloarSystemId == solardSystemId && x.MapId == mapid);
            }
        }
    }
}

