using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAdmins;

namespace WHMapper.Repositories.WHAccesses
{
    public class WHAccessRepository : ADefaultRepository<WHMapperContext, WHAccess, int>, IWHAccessRepository
    {
        public WHAccessRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHAccess?> ACreate(WHAccess item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHAccesses.AddAsync(item);
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
                var deleteRow = await _dbContext.DbWHAccesses.Where(x => x.Id == id).ExecuteDeleteAsync();
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

        protected override async Task<IEnumerable<WHAccess>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHAccesses.ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHAccess?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHAccesses.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }

        }

        protected override async Task<WHAccess?> AUpdate(int id, WHAccess item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext.DbWHAccesses.Update(item);
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

