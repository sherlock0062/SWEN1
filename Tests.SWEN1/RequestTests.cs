namespace Tests.SWEN1;

public class RequestTests
{
    private TestHelpers _testHelpers;

    [SetUp]
    public void Setup()
    {
        _testHelpers = new TestHelpers();
    }

    [Test]
    public void CreateHttpRequest_ValidRequest_ParsesMethod()
    {
        var rawRequest = _testHelpers.BuildRawRequest("GET", "/cards");
        var request = _testHelpers.CreateHttpRequest(rawRequest);
        Assert.That(request.Method, Is.EqualTo("GET"));
    }
    
    [Test]
    public void CreateHttpRequest_ValidRequest_ParsesMethodAndPath()
    {
        var rawRequest = _testHelpers.BuildRawRequest("POST", "/users", "{\"Username\":\"test\",\"Password\":\"123\"}");
        var request = _testHelpers.CreateHttpRequest(rawRequest);
        Assert.That(request.Method, Is.EqualTo("POST"));
        Assert.That(request.Path, Is.EqualTo("/users"));
    }
    
    [Test]
    public void CreateHttpRequest_WithQueryParams_ParsesPathCorrectly()
    {
        var rawRequest = _testHelpers.BuildRawRequest("GET", "/cards");
        var request = _testHelpers.CreateHttpRequest(rawRequest);
        Assert.That(request.Method, Is.EqualTo("GET"));
        Assert.That(request.Path, Is.EqualTo("/cards"));
    }

    [Test]
    public void CreateHttpRequest_WithJsonBody_ParsesBodyCorrectly()
    {
        var body = "{\"Username\":\"test\",\"Password\":\"123\"}";
        var rawRequest = _testHelpers.BuildRawRequest("POST", "/users", body);
        var request = _testHelpers.CreateHttpRequest(rawRequest);
        Assert.That(request.Body, Is.EqualTo(body));
    }
    
    [Test]
    public void CreateHttpRequest_EmptyBody_ParsesBodyAsEmptyString()
    {
        var rawRequest = _testHelpers.BuildRawRequest("POST", "/users");
        var request = _testHelpers.CreateHttpRequest(rawRequest);
        Assert.That(request.Body, Is.EqualTo(""));
    }
}
