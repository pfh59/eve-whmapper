namespace WHMapper.Models.DTO.EveMapper
{
    public class EveSystemEffect
    {
		public string Name { get; private set; }
		/// <summary>
		/// Value is in %
		/// </summary>
        public int Value { get; private set; }

        public EveSystemEffect(string name,int value)
		{
			Name = name;
			Value = value;
		}
	}
}


