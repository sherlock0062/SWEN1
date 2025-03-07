namespace Tests.SWEN1;

public class AuthTokenTests
{
    private TestHelpers _testHelpers;

    [SetUp]
    public void Setup()
    {
        _testHelpers = new TestHelpers();
    }

    [Test]
    public void ExtractAuthToken_ValidHeader_ReturnsToken()
    {
        var token = _testHelpers.ExtractAuthToken(TestHelpers.ValidBearerHeader);
        Assert.That(token, Is.EqualTo(TestHelpers.SampleToken));
    }

    [Test]
    public void ExtractAuthToken_EmptyHeader_ReturnsEmpty()
    {
        var token = _testHelpers.ExtractAuthToken("");
        Assert.That(token, Is.EqualTo(""));
    }

    [Test]
    public void ExtractAuthToken_MissingBearerPrefix_ReturnsEmpty()
    {
        var token = _testHelpers.ExtractAuthToken(TestHelpers.SampleToken);
        Assert.That(token, Is.EqualTo(""));
    }

    [Test]
    public void ExtractAuthToken_MalformedHeader_ReturnsEmpty()
    {
        var token = _testHelpers.ExtractAuthToken("Bearer");
        Assert.That(token, Is.EqualTo(""));
    }

    [Test]
    public void ExtractAuthToken_TokenWithSpaces_TrimsToken()
    {
        var token = _testHelpers.ExtractAuthToken("Bearer " + TestHelpers.SampleToken + " ");
        Assert.That(token, Is.EqualTo(TestHelpers.SampleToken));
    }
}