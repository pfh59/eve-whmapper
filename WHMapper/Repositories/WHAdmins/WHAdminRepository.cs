using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Repositories.WHAdmins
{
	public class WHAdminRepository : ADefaultRepository<WHMapperContext, WHAdmin, int>, IWHAdminRepository
    {
        public WHAdminRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHAdmin?> ACreate(WHAdmin item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHAdmins.AddAsync(item);
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
                var deleteRow = await _dbContext.DbWHAdmins.Where(x => x.Id == id).ExecuteDeleteAsync();
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

        protected override async Task<IEnumerable<WHAdmin>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHAdmins.ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHAdmin?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHAdmins.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }

        }

        protected override async Task<WHAdmin?> AUpdate(int id, WHAdmin item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext.DbWHAdmins.Update(item);
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

