using WHMapper.Services.Anoik;

namespace WHMapper.Tests.Services.Anoik
{
    public class AnoikServiceDataSupplierTests
    {
        private const string ValidJsonFilePath = "validData.json";
        private const string InvalidJsonFilePath = "invalidData.json";
        private const string NonExistentJsonFilePath = "nonExistentData.json";
        private const string InvalidJsonContent = "{ \"invalid\": [";

        public AnoikServiceDataSupplierTests()
        {
            File.WriteAllText(ValidJsonFilePath, AnoikServiceTestConstants.ValidJson);
            File.WriteAllText(InvalidJsonFilePath, InvalidJsonContent);
        }

        [Fact]
        public void Constructor_ValidFilePath_ShouldInitialize()
        {
            var supplier = new AnoikJsonDataSupplier(ValidJsonFilePath);
            Assert.NotNull(supplier);
        }

        [Fact]
        public void Constructor_NullFilePath_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AnoikJsonDataSupplier(null!));
        }

        [Fact]
        public void Constructor_EmptyFilePath_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentException>(() => new AnoikJsonDataSupplier(""));
        }

        [Fact]
        public void Constructor_NonExistentFilePath_ShouldThrowArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new AnoikJsonDataSupplier(NonExistentJsonFilePath));
            Assert.IsType<FileNotFoundException>(exception.InnerException);
        }

        [Fact]
        public void Constructor_InvalidJsonFile_ShouldThrowArgumentException()
        {
            var exceptio0n = Assert.Throws<ArgumentException>(() => new AnoikJsonDataSupplier(InvalidJsonFilePath));
        }

        [Fact]
        public void GetSystems_ValidJson_ShouldReturnSystems()
        {
            var sut = new AnoikJsonDataSupplier(ValidJsonFilePath);

            var systems = sut.GetSystems();

            Assert.True(systems.TryGetProperty("J120450", out var j120450));
            Assert.Equal(31001554, j120450.GetProperty("solarSystemID").GetInt32());
        }

        [Fact]
        public void GetEffect_ValidJson_ShouldReturnEffects()
        {
            var sut = new AnoikJsonDataSupplier(ValidJsonFilePath);

            var effects = sut.GetEffect();

            Assert.True(effects.TryGetProperty("Pulsar", out var pulsar));
            Assert.Equal("+30%", pulsar.GetProperty("Shield Capacity").EnumerateArray().ElementAt(0).GetString());
        }

        [Fact]
        public void GetWormHoles_ValidJson_ShouldReturnWormHoles()
        {
            var sut = new AnoikJsonDataSupplier(ValidJsonFilePath);

            var wormholes = sut.GetWormHoles();

            Assert.True(wormholes.TryGetProperty("H900", out var k162));
            Assert.Equal("C5", k162.GetProperty("dest").GetString());
        }
    }
}
