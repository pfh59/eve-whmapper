using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;

namespace WHMapper.Repositories.WHSystems
{
    public class WHSystemRepository : ADefaultRepository<WHMapperContext, WHSystem, int>, IWHSystemRepository
    {
        public WHSystemRepository(WHMapperContext context) : base(context)
        {
        }

        protected override async Task<WHSystem?> ACreate(WHSystem item)
        {
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
        }

        protected override async Task<WHSystem?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.DbWHSystems.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHSystem>?> AGetAll()
        {

            if (_dbContext?.DbWHSystems?.Count() == 0)
                return await _dbContext.DbWHSystems.ToListAsync();
            else
                return await _dbContext?.DbWHSystems?.OrderBy(x => x.Name)
                        ?.Include(x => x.WHSignatures)
                        ?.ToListAsync();
        }

        protected override async Task<WHSystem?> AGetById(int id)
        {
            return await _dbContext?.DbWHSystems
                .Include(x => x.WHSignatures)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override async Task<WHSystem?> AUpdate(int id, WHSystem item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task<WHSystem?> GetByName(string name)
        {
            return await _dbContext?.DbWHSystems?.Include(x => x.WHSignatures)?.FirstOrDefaultAsync(x => x.Name == name);
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
            await _dbContext?.DbWHSignatures?.AddRangeAsync(sigArray);

            foreach (var sig in sigArray)
                system.WHSignatures.Add(sig);


            await this.Update(idWHSystem, system);

            return whSignatures;
        }


        public async Task<WHSignature?> RemoveWHSignature(int idWHSystem, int idWHSignature)
        {
            var system = await this.GetById(idWHSystem);
            if (system == null)
                return null;

            var sig = system.WHSignatures.Where(x => x.Id == idWHSignature).FirstOrDefault();
            if (sig == null)
                return null;

            _dbContext?.DbWHSignatures?.Remove(sig);
            await _dbContext?.SaveChangesAsync();

            return sig;

        }

    }
}

