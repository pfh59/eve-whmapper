using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
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

        protected override async Task<WHSystem?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            await semSlim.WaitAsync();
            try
            {

                _dbContext.DbWHSystems.Remove(item);
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

        public async Task<WHSignature?> AddWHSignature(int idWHSystem, WHSignature whSignature)
        {
            var system = await this.GetById(idWHSystem);
            if (system == null)
                return null;


            _dbContext?.DbWHSignatures?.Add(whSignature);
            system.WHSignatures.Add(whSignature);

            await this.Update(idWHSystem, system);

            return whSignature;
        }

        public async Task<IEnumerable<WHSignature?>> AddWHSignatures(int idWHSystem, IEnumerable<WHSignature> whSignatures)
        {
            var system = await this.GetById(idWHSystem);
            if (system == null)
                return null;

            var sigArray = whSignatures.ToArray();
            await _dbContext.DbWHSignatures.AddRangeAsync(sigArray);

            foreach (var sig in sigArray)
                system.WHSignatures.Add(sig);


            if (await this.Update(idWHSystem, system) != null)
                return whSignatures;
            else
            {
                foreach (var sig in sigArray)
                    system.WHSignatures.Remove(sig);

                return null;
            }
        }


        public async Task<WHSignature?> RemoveWHSignature(int idWHSystem, int idWHSignature)
        {
            var system = await this.GetById(idWHSystem);
            if (system == null)
                return null;

            await semSlim.WaitAsync();
            try
            {
                var sig = system.WHSignatures.Where(x => x.Id == idWHSignature).FirstOrDefault();
                if (sig == null)
                    return null;

                _dbContext.DbWHSignatures?.Remove(sig);
                await _dbContext.SaveChangesAsync();
                return sig;
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

        public async Task<bool> RemoveAllWHSignature(int idWHSystem)
        {
            var system = await this.GetById(idWHSystem);
            if (system == null)
                return false;

            await semSlim.WaitAsync();
            try
            {
                _dbContext.DbWHSignatures.RemoveRange(system.WHSignatures);
                await _dbContext.SaveChangesAsync();
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

    }
}

