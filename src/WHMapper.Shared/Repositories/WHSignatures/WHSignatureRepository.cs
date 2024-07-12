using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WHMapper.Shared.Data;
using WHMapper.Shared.Models.Db;


namespace WHMapper.Shared.Repositories.WHSignatures
{

    public class WHSignatureRepository : ADefaultRepository<WHMapperContext, WHSignature, int>, IWHSignatureRepository
    {
        public WHSignatureRepository(ILogger<WHSignatureRepository> logger, IDbContextFactory<WHMapperContext> context)
            : base(logger, context)
        {
        }


        protected override async Task<WHSignature?> ACreate(WHSignature item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHSignatures.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHSignature : {Name}", item.Name);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int rowDeleted = await context.DbWHSignatures.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (rowDeleted > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHSignature>?> AGetAll()
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (!await context.DbWHSignatures.AnyAsync())
                    return await context.DbWHSignatures.ToListAsync();
                else
                    return await context.DbWHSignatures.OrderBy(x => x.Id)
                            .ToListAsync();
            }
        }

        protected override async Task<WHSignature?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSignatures.SingleOrDefaultAsync(x => x.Id == id);
                ;
            }
        }

        protected override async Task<WHSignature?> AUpdate(int id, WHSignature item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHSignatures.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update WHSignature : {Name}", item.Name);
                    return null;
                }
            }
        }

        public async Task<WHSignature?> GetByName(string name)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSignatures.FirstOrDefaultAsync(x => x.Name == name);
            }

        }

        public async Task<IEnumerable<WHSignature?>?> Update(IEnumerable<WHSignature> whSignatures)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {

                    foreach (var sig in whSignatures)
                    {
                        context.Entry(sig).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();

                    return whSignatures;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update multiple WHSignatures");
                    return null;
                }
            }
        }

        public async Task<IEnumerable<WHSignature>?> GetByWHId(int whid)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (!await context.DbWHSignatures.AnyAsync())
                    return await context.DbWHSignatures.ToListAsync();
                else
                {
                    return await context.DbWHSignatures.Where(x => x.WHId == whid).OrderBy(x => x.Id)
                            .ToListAsync();
                }
            }
        }

        public async Task<bool> DeleteByWHId(int whid)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                int rowDeleted = await context.DbWHSignatures.Where(x => x.WHId == whid).ExecuteDeleteAsync();
                if (rowDeleted > 0)
                    return true;
                else
                    return false;
            }
        }

        public async Task<IEnumerable<WHSignature?>?> Create(IEnumerable<WHSignature> whSignatures)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    var sigArray = whSignatures.ToArray();
                    await context.DbWHSignatures.AddRangeAsync(sigArray);
                    await context.SaveChangesAsync();


                    return whSignatures;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create multiple WHSignatures");
                    return null;
                }
            }
        }
    }

}

