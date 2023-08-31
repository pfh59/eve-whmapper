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
                int deleteRow = await context.DbWHSystems.Where(x => x.Id == id).ExecuteDeleteAsync();
                if (deleteRow > 0)
                    return true;
                else
                    return false;
            }
        }

        protected override async Task<IEnumerable<WHSystem>?> AGetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.DbWHSystems.Count() == 0)
                    return await context.DbWHSystems.ToListAsync();
                else
                    return await context.DbWHSystems.OrderBy(x => x.Name)
                            .Include(x => x.WHSignatures)
                            .ToListAsync();
            }
        }

        protected override async Task<WHSystem?> AGetById(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.DbWHSystems
                        .Include(x => x.WHSignatures)
                        .SingleOrDefaultAsync(x => x.Id == id);
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
                return await context.DbWHSystems.Include(x => x.WHSignatures).FirstOrDefaultAsync(x => x.Name == name);
            }
        }
    }
}

