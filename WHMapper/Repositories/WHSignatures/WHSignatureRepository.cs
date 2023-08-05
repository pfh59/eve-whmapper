using System;
using Microsoft.EntityFrameworkCore;
using WHMapper.Data;
using WHMapper.Models.Db;
using static MudBlazor.CategoryTypes;


namespace WHMapper.Repositories.WHSignatures
{

    public class WHSignatureRepository : ADefaultRepository<WHMapperContext, WHSignature, int>, IWHSignatureRepository
    {
        public WHSignatureRepository(IDbContextFactory<WHMapperContext> context)
            : base(context)
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
                    await context.DbWHSignatures.Where(x => x.Id == id).ExecuteDeleteAsync();
                    return true;
                }
                catch (Exception ex)
                {
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
                    return null;
                }
            }

        }

        public async Task<IEnumerable<WHSignature?>> Update(IEnumerable<WHSignature> whSignatures)
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
                        return await context.DbWHSignatures.Where(x => x.WHId == whid).OrderBy(x => x.Id)
                                .ToListAsync();

                }
                catch (Exception ex)
                {
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
                    return false;
                }
            }
        }

        public async Task<IEnumerable<WHSignature?>> Create(IEnumerable<WHSignature> whSignatures)
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
                    return null;
                }
            }
        }
    }
    
}

