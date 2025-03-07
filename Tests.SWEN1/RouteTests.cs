namespace Tests.SWEN1;

[TestFixture]
public class RouteTests
{
    private TestHelpers _testHelpers;

    [SetUp]
    public void Setup()
    {
        _testHelpers = new TestHelpers();
    }

    [Test]
    public void HandleMockRoute_PostUsers_ValidBody_Returns201()
    {
        var body = "{\"Username\":\"newUser\",\"Password\":\"123\"}";
        var result = _testHelpers.HandleMockRoute("POST", "/users", "", "", body);
        Assert.That(result.Item1, Is.EqualTo(201));
    }

    [Test]
    public void HandleMockRoute_GetCards_EmptyDeck_Returns200()
    {
        var result = _testHelpers.HandleMockRoute("GET", "/cards", "empty_deck=true", TestHelpers.SomeUserToken, "");
        Assert.That(result.Item1, Is.EqualTo(200));
    }

    [Test]
    public void HandleMockRoute_PostPackages_AdminToken_Returns201()
    {
        var body =
            "[{\"Id\":\"00000000-0000-0000-0000-000000000001\",\"Name\":\"FireGoblin\",\"Damage\":10}," +
            "{\"Id\":\"00000000-0000-0000-0000-000000000002\",\"Name\":\"FireGoblin2\",\"Damage\":10}," +
            "{\"Id\":\"00000000-0000-0000-0000-000000000003\",\"Name\":\"FireGoblin3\",\"Damage\":10}," +
            "{\"Id\":\"00000000-0000-0000-0000-000000000004\",\"Name\":\"FireGoblin4\",\"Damage\":10}," +
            "{\"Id\":\"00000000-0000-0000-0000-000000000005\",\"Name\":\"FireGoblin5\",\"Damage\":10}]";
        var result = _testHelpers.HandleMockRoute("POST", "/packages", "", TestHelpers.AdminToken, body);
        Assert.That(result.Item1, Is.EqualTo(201));
    }

    [Test]
    public void HandleMockRoute_GetDeck_ValidUser_Returns200()
    {
        var result = _testHelpers.HandleMockRoute("GET", "/deck", "", TestHelpers.SomeUserToken, "");
        Assert.That(result.Item1, Is.EqualTo(200));
    }

    [Test]
    public void HandleMockRoute_GetScoreboard_ValidUser_Returns200()
    {
        var result = _testHelpers.HandleMockRoute("GET", "/scoreboard", "", TestHelpers.SomeUserToken, "");
        Assert.That(result.Item1, Is.EqualTo(200));
    }

}