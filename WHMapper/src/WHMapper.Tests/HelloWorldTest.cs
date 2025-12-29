using Xunit;

public class HelloWorldTest
{
    [Fact]
    public void TestHelloWorld()
    {
        Assert.Equal("Hello, World!", GetHelloWorld());
    }

    private string GetHelloWorld()
    {
        return "Hello, World!";
    }
}