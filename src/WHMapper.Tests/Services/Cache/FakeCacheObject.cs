namespace WHMapper.Tests.Services.Cache
{
    public  class FakeCacheObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is FakeCacheObject other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
