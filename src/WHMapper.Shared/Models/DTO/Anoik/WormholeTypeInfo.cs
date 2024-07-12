namespace WHMapper.Shared.Models.DTO.Anoik
{
    public class WormholeTypeInfo
    {
        public string Name { private set; get; }
        public string? Destination { private set; get; }
        public string[]? Sources { private set; get; }

        public WormholeTypeInfo(string name, string? dest, string[]? srcs)
        {
            Name = name;
            Destination = !string.IsNullOrEmpty(dest) ? dest.ToUpper() : null;
            Sources = srcs != null ? srcs.Select(x => x.ToUpper()).ToArray() : null;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Destination))
                return Name;
            else
                return string.Format("{0} -> {1}", Name, Destination);

        }
    }
}

