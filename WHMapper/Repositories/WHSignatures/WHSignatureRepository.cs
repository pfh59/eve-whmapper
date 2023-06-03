using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using static MudBlazor.CategoryTypes;


namespace WHMapper.Repositories.WHSignatures
{

    public class WHSignatureRepository : ADefaultRepository<WHMapperContext, WHSignature, int>, IWHSignatureRepository
    {
        public WHSignatureRepository(WHMapperContext context) : base(context)
        {
        }


        protected override async Task<WHSignature?> ACreate(WHSignature item)
        {
            await semSlim.WaitAsync();
            try
            {
                await _dbContext.DbWHSignatures.AddAsync(item);
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
                await _dbContext.DbWHSignatures.Where(x => x.Id == id).ExecuteDeleteAsync();
                return true;
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

        protected override async Task<IEnumerable<WHSignature>?> AGetAll()
        {
            await semSlim.WaitAsync();
            try
            {
                if (_dbContext.DbWHSignatures.Count() == 0)
                    return await _dbContext.DbWHSignatures.ToListAsync();
                else
                    return await _dbContext.DbWHSignatures.OrderBy(x => x.Id)
                            .ToListAsync();
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHSignature?> AGetById(int id)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHSignatures.FindAsync(id);
            }
            finally
            {
                semSlim.Release();
            }
        }

        protected override async Task<WHSignature?> AUpdate(int id, WHSignature item)
        {
            await semSlim.WaitAsync();
            try
            {
                if (id != item.Id)
                    return null;

                _dbContext.DbWHSignatures.Update(item);
                await _dbContext.SaveChangesAsync();
                return item;
            }
            finally
            {
                 semSlim.Release();
            }
        }

        public async Task<WHSignature?> GetByName(string name)
        {
            await semSlim.WaitAsync();
            try
            {
                return await _dbContext.DbWHSignatures.FirstOrDefaultAsync(x => x.Name == name);
            }
            finally
            {
                semSlim.Release();
            }

        }

        public async Task<IEnumerable<WHSignature?>> Update(IEnumerable<WHSignature> whSignatures)
        {
            await semSlim.WaitAsync();
            try
            {

                foreach (var sig in whSignatures)
                {
                    _dbContext.Entry(sig).State = EntityState.Modified;
                }

                await _dbContext.SaveChangesAsync();

                return whSignatures;
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                semSlim.Release();
            }
        }

        public async Task<IEnumerable<WHSignature>?> GetByWHId(int whid)
        {
            await semSlim.WaitAsync();
            try
            {
                if (_dbContext.DbWHSignatures.Count() == 0)
                    return await _dbContext.DbWHSignatures.ToListAsync();
                else
                    return await _dbContext.DbWHSignatures.Where(x => x.WHId == whid).OrderBy(x => x.Id)
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

        public async Task<bool> DeleteByWHId(int whid)
        {
            await semSlim.WaitAsync();
            try
            {
                int rowDeleted = await _dbContext.DbWHSignatures.Where(x => x.WHId == whid).ExecuteDeleteAsync();
                if (rowDeleted > 0)
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

        public async Task<IEnumerable<WHSignature?>> Create(IEnumerable<WHSignature> whSignatures)
        {
            await semSlim.WaitAsync();
            try
            {
                var sigArray = whSignatures.ToArray();
                await _dbContext.DbWHSignatures.AddRangeAsync(sigArray);
                await _dbContext.SaveChangesAsync();


                return whSignatures;
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

