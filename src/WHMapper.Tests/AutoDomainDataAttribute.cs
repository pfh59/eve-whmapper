using AutoFixture;
using AutoFixture.Xunit2;

namespace WHMapper.Tests
{
    public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(() => new Fixture().Customize(new DomainCustomization()))
        {
        }
    }
}
