using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;


namespace WHMapper.Repositories.WHSignatures
{

    public class WHSignatureRepository : ADefaultRepository<WHMapperContext, WHSignature, string>, IWHSignatureRepository
    {
        public WHSignatureRepository(WHMapperContext context) : base(context)
        {
        }

        
        protected override async Task<WHSignature?> ACreate(WHSignature item)
        {
            try
            {
                await _dbContext.WHSignatures.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                return item;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        protected override async Task<WHSignature?> ADeleteById(string id)
        {
            var item = await AGetById(id);

            if (item == null)
                return null;

            _dbContext.WHSignatures.Remove(item);
            await _dbContext.SaveChangesAsync();

            return item;
        }

        protected override async Task<IEnumerable<WHSignature>?> AGetAll()
        {
            if (_dbContext.WHSignatures.Count() == 0)
                return await _dbContext.WHSignatures.ToListAsync();
            else
                return await _dbContext.WHSignatures.OrderBy(x => x.Id)
                        .ToListAsync();
        }

        protected override async Task<WHSignature?> AGetById(string id)
        {
            return await _dbContext.WHSignatures
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override async Task<WHSignature?> AUpdate(string id, WHSignature item)
        {
            if (id != item.Id)
                return null;

            _dbContext.Entry(item).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return item;
        }
    }
    
}

