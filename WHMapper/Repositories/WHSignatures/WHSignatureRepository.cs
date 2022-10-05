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
        }

        protected override async Task<WHSignature?> ADeleteById(int id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.DbWHSignatures.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHSignature>?> AGetAll()
        {
            if (_dbContext.DbWHSignatures.Count() == 0)
                return await _dbContext.DbWHSignatures.ToListAsync();
            else
                return await _dbContext.DbWHSignatures.OrderBy(x => x.Id)
                        .ToListAsync();
        }

        protected override async Task<WHSignature?> AGetById(int id)
        {
            return await _dbContext.DbWHSignatures.FindAsync(id);
        }

        protected override async Task<WHSignature?> AUpdate(int id, WHSignature item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task<WHSignature?> GetByName(string name)
        {
            return await _dbContext.DbWHSignatures.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<IEnumerable<WHSignature?>> Update(IEnumerable<WHSignature> whSignatures)
        {
            try
            {
                foreach (var sig in whSignatures)
                {
                    _dbContext.Entry(sig).State = EntityState.Modified;
                }

                await _dbContext.SaveChangesAsync();
                return whSignatures;
            }
            catch
            {
                return null;
            }
        }
    }
    
}

