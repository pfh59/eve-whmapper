using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAccesses;
using YamlDotNet.Core;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Repositories.WHSystems
{
    public class WHSystemRepository : ADefaultRepository<WHMapperContext, WHSystem, int>, IWHSystemRepository
    {
        public WHSystemRepository(ILogger<WHSystemRepository> logger,IDbContextFactory<WHMapperContext> context)
            : base(logger,context)
        {
        }

        protected override async Task<WHSystem?> ACreate(WHSystem item)
        {

            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    await context.DbWHSystems.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to create WHSystem : {0}", item.Name));
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
                    int deleteRow = await context.DbWHSystems.Where(x => x.Id == id).ExecuteDeleteAsync();
                    if (deleteRow > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to delete WHSystem id : {0}", id));
                    return false;
                }
            }
        }

        protected override async Task<IEnumerable<WHSystem>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (context.DbWHSystems.Count() == 0)
                        return await context.DbWHSystems.ToListAsync();
                    else
                        return await context.DbWHSystems.OrderBy(x => x.Name)
                                .Include(x => x.WHSignatures)
                                .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to get all WHSystems id : {0}");
                    return null;
                }
            }
        }

        protected override async Task<WHSystem?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    var res = await context.DbWHSystems
                            .Include(x => x.WHSignatures)
                            .SingleOrDefaultAsync(x => x.Id == id);

                    return res;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get WHSystem by id : {0}", id));
                    return null;
                }
            }

        }

        protected override async Task<WHSystem?> AUpdate(int id, WHSystem item)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    if (id != item.Id)
                        return null;

                    context.DbWHSystems.Update(item);
                    await context.SaveChangesAsync();
                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to udpate WHSystem : {0}", item.Name));
                    return null;
                }
            }
        }

        public async Task<WHSystem?> GetByName(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                try
                {
                    return await context.DbWHSystems.Include(x => x.WHSignatures).FirstOrDefaultAsync(x => x.Name == name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, String.Format("Impossible to get by name WHSystem : {0}", name));
                    return null;
                }
            }
        }
    }
}

