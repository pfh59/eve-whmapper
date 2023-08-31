using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAccesses;
using static MudBlazor.CategoryTypes;


namespace WHMapper.Repositories.WHSignatures
{

    public class WHSignatureRepository : ADefaultRepository<WHMapperContext, WHSignature, int>, IWHSignatureRepository
    {
        public WHSignatureRepository(ILogger<WHSignatureRepository> logger,IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }


        protected override async Task<WHSignature?> ACreate(WHSignature item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHSignatures.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHSignature : {0}", item.Name));
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    int rowDeleted = await context.DbWHSignatures.Where(x => x.Id == id).ExecuteDeleteAsync();
                    if (rowDeleted > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to delete WHSignature by id : {0}",id));
                    return false;
                }
            }
        }

        protected override async Task<IEnumerable<WHSignature>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (context.DbWHSignatures.Count() == 0)
                        return await context.DbWHSignatures.ToListAsync();
                    else
                        return await context.DbWHSignatures.OrderBy(x => x.Id)
                                .ToListAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Impossible to get all WHSignatures");
                    return null;
                }
            }
        }

        protected override async Task<WHSignature?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHSignatures.FindAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHSignature by id : {0}", id));
                    return null;
                }
            }
        }

        protected override async Task<WHSignature?> AUpdate(int id, WHSignature item)
        {
            using (var context = _contextFactory.CreateDbContext())
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
                    _logger.LogError(ex, String.Format("Impossible to update WHSignature : {0}", item.Name));
                    return null;
                }
            }
        }

        public async Task<WHSignature?> GetByName(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHSignatures.FirstOrDefaultAsync(x => x.Name == name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHSignature by name : {0}", name));
                    return null;
                }
            }

        }

        public async Task<IEnumerable<WHSignature?>?> Update(IEnumerable<WHSignature> whSignatures)
        {
            using (var context = _contextFactory.CreateDbContext())
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
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Impossible to update multiple WHSignatures");
                    return null;
                }
            }
        }

        public async Task<IEnumerable<WHSignature>?> GetByWHId(int whid)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (context.DbWHSignatures.Count() == 0)
                        return await context.DbWHSignatures.ToListAsync();
                    else
                    {
                        return await context.DbWHSignatures.Where(x => x.WHId == whid).OrderBy(x => x.Id)
                                .ToListAsync();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHSignature by WH id : {0}", whid));

                    return null;
                }
            }
        }

        public async Task<bool> DeleteByWHId(int whid)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    int rowDeleted = await context.DbWHSignatures.Where(x => x.WHId == whid).ExecuteDeleteAsync();
                    if (rowDeleted > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to delete WHSignature by WH id : {0}", whid));

                    return false;
                }
            }
        }

        public async Task<IEnumerable<WHSignature?>?> Create(IEnumerable<WHSignature> whSignatures)
        {
            using (var context = _contextFactory.CreateDbContext())
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

