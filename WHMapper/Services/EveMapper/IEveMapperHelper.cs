using System;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;

namespace WHMapper.Services.EveMapper
{
	public interface IEveMapperHelper
	{
        public Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh);

    }
}

