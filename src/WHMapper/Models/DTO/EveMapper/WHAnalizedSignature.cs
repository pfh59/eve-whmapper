using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper
{


    public class WHAnalizedSignature
    {
        private readonly WHMapper.Models.Db.WHSignature _signature;
        public WHAnalizedSignatureEnums Status { get; private set;}

        public string Name { get { return _signature.Name; } }
        public string Group { get { return _signature.Group.ToString(); } }
        public string? Type { get { return _signature.Type; } }
        
        public WHAnalizedSignature(WHMapper.Models.Db.WHSignature whsig,WHAnalizedSignatureEnums status)
        {
            _signature= whsig;
            Status = status;
        }
    }
}

