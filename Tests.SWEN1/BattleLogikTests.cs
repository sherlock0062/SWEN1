namespace Tests.SWEN1;

public class BattleLogikTests
{
    private TestHelpers _testHelpers;

    [SetUp]
    public void Setup()
    {
        _testHelpers = new TestHelpers();
    }

    [Test]
    public void CalculateAttackDamage_GoblinVsDragon_ReturnsZero()
    {
        var goblin = new TestHelpers.FakeCard { Name = "Goblin", Damage = 10 };
        var dragon = new TestHelpers.FakeCard { Name = "Dragon", Damage = 50 };
        var dmg = _testHelpers.CalculateAttackDamage(goblin, dragon);
        Assert.That(dmg, Is.EqualTo(0));
    }

    [Test]
    public void CalculateAttackDamage_OrkVsWizzard_ReturnsZero()
    {
        var wizzard = new TestHelpers.FakeCard { Name = "Ork", Damage = 20 };
        var ork = new TestHelpers.FakeCard { Name = "Wizzard", Damage = 30 };
        var dmg = _testHelpers.CalculateAttackDamage(wizzard, ork);
        Assert.That(dmg, Is.EqualTo(0));
    }

    [Test]
    public void CalculateAttackDamage_NormalVsNormal_ReturnsBaseDamage()
    {
        var normal1 = new TestHelpers.FakeCard
            { Name = "MonsterA", Damage = 10, ElementType = "Normal", CardType = "Monster" };
        var normal2 = new TestHelpers.FakeCard
            { Name = "MonsterB", Damage = 15, ElementType = "Normal", CardType = "Monster" };
        var dmg = _testHelpers.CalculateAttackDamage(normal1, normal2);
        Assert.That(dmg, Is.EqualTo(10));
    }

    [Test]
    public void CalculateAttackDamage_WaterVsFire_ReturnsDoubleDamageElementalAdvantage()
    {
        var water = new TestHelpers.FakeCard
            { Name = "WaterSpell", Damage = 20, ElementType = "Water", CardType = "Spell" };
        var fire = new TestHelpers.FakeCard
            { Name = "FireElf", Damage = 15, ElementType = "Fire", CardType = "Monster" };
        var dmg = _testHelpers.CalculateAttackDamage(water, fire);
        Assert.That(dmg, Is.EqualTo(40));
    }

    [Test]
    public void CalculateAttackDamage_FireVsWater_ReturnsHalfDamageElementalDisadvantage()
    {
        var fire = new TestHelpers.FakeCard
            { Name = "FireSpell", Damage = 20, ElementType = "Fire", CardType = "Spell" };
        var water = new TestHelpers.FakeCard
            { Name = "WaterElf", Damage = 15, ElementType = "Water", CardType = "Monster" };
        var dmg = _testHelpers.CalculateAttackDamage(fire, water);
        Assert.That(dmg, Is.EqualTo(10));
    }

}