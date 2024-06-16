namespace WHMapper.Services.SDE
{
    public interface ISDEDataSupplier
    {
        public string GetChecksum();
        Task<Stream> GetSDEDataStreamAsync();
    }
}
