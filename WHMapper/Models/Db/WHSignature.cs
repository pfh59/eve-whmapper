using System;
namespace WHMapper.Models.Db
{
    public class WHSignature
    {
        public string Id { get; private set; }
        public string Group { get; private set; }
        public string Destination { get; private set; }
        public DateTime Created { get; private set; }
        public string CreatedBy { get; private set; }
        public string Updated { get; private set; }
        public DateTime UpdatedBy { get; private set; }


        public WHSignature()
        {
        }
    }
}

