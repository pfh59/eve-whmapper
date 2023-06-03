using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHMaps
{
    public class WHMapRepository : ADefaultRepository<WHMapperContext, WHMap, int>, IWHMapRepository
    {
        public WHMapRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHMap?> ACreate(WHMap item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHMaps.AddAsync(item);
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

        protected override async Task<bool> ADeleteById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                var deleteRow = await _dbContext.DbWHMaps.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<IEnumerable<WHMap>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {

                if (_dbContext.DbWHMaps.Count() == 0)
                    return await _dbContext.DbWHMaps.ToListAsync();
                else
                    return await _dbContext.DbWHMaps
                            .Include(x => x.WHSystems)
                            .Include(x => x.WHSystemLinks)
                            .OrderBy(x => x.Name).ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHMap?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHMaps.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }

        }

        protected override async Task<WHMap?> AUpdate(int id, WHMap item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext?.DbWHMaps?.Update(item);
                await _dbContext?.SaveChangesAsync();
                return item;
            }
            finally
            {
                semSlim.Release();
            }
        }
    }
}

