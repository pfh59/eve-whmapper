using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using YamlDotNet.Core;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHSystems
{
    public class WHSystemRepository : ADefaultRepository<WHMapperContext, WHSystem, int>, IWHSystemRepository
    {
        public WHSystemRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHSystem?> ACreate(WHSystem item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHSystems.AddAsync(item);
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
                await _dbContext.DbWHSystems.Where(x => x.Id == id).ExecuteDeleteAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<IEnumerable<WHSystem>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {
                if (_dbContext.DbWHSystems.Count() == 0)
                    return await _dbContext.DbWHSystems.ToListAsync();
                else
                    return await _dbContext.DbWHSystems.OrderBy(x => x.Name)
                            .Include(x => x.WHSignatures)
                            .ToListAsync();
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

        protected override async Task<WHSystem?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                var res = await _dbContext.DbWHSystems
                        .Include(x=>x.WHSignatures)
                        .SingleOrDefaultAsync(x => x.Id == id);
    
                return res;
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

        protected override async Task<WHSystem?> AUpdate(int id, WHSystem item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext.DbWHSystems.Update(item);
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

        public async Task<WHSystem?> GetByName(string name)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHSystems.Include(x => x.WHSignatures).FirstOrDefaultAsync(x => x.Name == name);
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
    }
}

