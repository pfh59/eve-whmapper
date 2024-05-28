using AutoFixture;
using AutoFixture.AutoMoq;

namespace WHMapper.Tests
{
    public class DomainCustomization : CompositeCustomization
    {
        public DomainCustomization() : base(
                new AutoMoqCustomization()
            )
        { }
    }
}
