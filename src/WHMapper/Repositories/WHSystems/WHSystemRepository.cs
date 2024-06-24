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

            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                try
                {
                    await context.DbWHSystems.AddAsync(item);
                    await context.SaveChangesAsync();

                    return item;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create WHSystem : {Name}", item.Name);
                    return null;
                }
            }
        }

        protected override async Task<bool> ADeleteById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
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
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                if (await context.DbWHSystems.CountAsync() == 0)
                    return await context.DbWHSystems.ToListAsync();
                else
                    return await context.DbWHSystems.OrderBy(x => x.Name)
                            .Include(x => x.WHSignatures)
                            .ToListAsync();
            }
        }

        protected override async Task<WHSystem?> AGetById(int id)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSystems
                        .Include(x => x.WHSignatures)
                        .SingleOrDefaultAsync(x => x.Id == id);
            }

        }

        protected override async Task<WHSystem?> AUpdate(int id, WHSystem item)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
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
                    _logger.LogError(ex, "Impossible to udpate WHSystem : {Name}", item.Name);
                    return null;
                }
            }
        }

        public async Task<WHSystem?> GetByName(string name)
        {
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.DbWHSystems.FirstOrDefaultAsync(x => x.Name == name);
            }
        }
    }
}

