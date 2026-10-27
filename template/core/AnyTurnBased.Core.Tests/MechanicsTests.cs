using System;
using System.Collections.Generic;
using System.Linq;
using AnyTurnBased.Core;
using Xunit;

public class MechanicsTests
{
    private const string Expedition33 = @"{
      ""$schema"": ""../mechanics/battle-config.schema.json"",
      ""version"": 1,
      ""preset"": ""expedition33"",
      ""time"": ""time.ctb"",
      ""mechanics"": [""economy.ap"", ""actions.menu"", ""reactive.dodge-parry"", ""defense.break"", ""party.solo""],
      ""params"": {
        ""economy.ap"": { ""start"": 1, ""max"": 5, ""perTurn"": 1, ""onBasicAttack"": 1, ""onPerfectReaction"": 1 },
        ""reactive.dodge-parry"": { ""windowMs"": 200, ""parryWindowMs"": 100, ""counterOnFullParry"": true }
      }
    }";

    private static Combatant Make(string name, Team team, int hp = 100, int atk = 20, int spd = 100) =>
        new Combatant(name, name, team, new Stats { MaxHp = hp, Hp = hp, Attack = atk, Speed = spd, MaxMp = 10, Mp = 10 }, null);

    private static BattleConfig Config(string json = Expedition33) => BattleConfig.FromJson(json);

    [Fact]
    public void ConfigReadsRulesAndListsWhatTheCoreLacks()
    {
        var c = Config();
        Assert.Equal(5, c.Ap.Max);
        Assert.Equal(0.2f, c.Reactions.DodgeWindow, 3);
        Assert.Equal(0.1f, c.Reactions.ParryWindow, 3);
        Assert.Equal(new[] { "defense.break" }, c.Unsupported);
        Assert.Null(BattleConfig.Default.Ap);
        Assert.Null(BattleConfig.Default.Reactions);
    }

    [Fact]
    public void UnimplementedTimeModelFailsLoud()
    {
        var ex = Assert.Throws<NotSupportedException>(() =>
            BattleConfig.FromJson(@"{""version"":1,""time"":""time.atb"",""mechanics"":[]}"));
        Assert.Contains("time.atb", ex.Message);
    }

    [Fact]
    public void WrongParamTypeFailsLoud()
    {
        Assert.Throws<FormatException>(() => BattleConfig.FromJson(
            @"{""version"":1,""time"":""time.ctb"",""mechanics"":[""economy.ap""],""params"":{""economy.ap"":{""max"":""lots""}}}"));
    }

    [Fact]
    public void ApStartsGrowsWithBasicAttacksAndPaysForSkills()
    {
        var p = Make("p", Team.Player, spd: 1000);
        var e = Make("e", Team.Enemy, hp: 10000, spd: 1);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1), Config());
        Assert.Equal(2, p.Ap);                       // start 1 + 1 at turn start

        b.Execute(new AttackAction(), e);            // +1 for the basic attack, +1 at the next turn start
        Assert.Equal(4, p.Ap);

        var skill = new SkillAction("Golpe", TargetKind.SingleEnemy, mpCost: 0, power: 2f, apCost: 3);
        Assert.True(b.CanAfford(p, skill));
        b.Execute(skill, e);
        Assert.Equal(2, p.Ap);                       // 4 - 3 + 1 at the next turn start

        var big = new SkillAction("Grande", TargetKind.SingleEnemy, mpCost: 0, power: 5f, apCost: 9);
        Assert.False(b.CanAfford(p, big));
        Assert.Throws<InvalidOperationException>(() => b.Execute(big, e));
    }

    [Fact]
    public void ApIsCappedAtMax()
    {
        var p = Make("p", Team.Player, spd: 1000);
        var e = Make("e", Team.Enemy, hp: 10000, spd: 1);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1), Config());
        for (int i = 0; i < 10; i++) b.Execute(new AttackAction(), e);
        Assert.Equal(5, p.Ap);
    }

    [Theory]
    [InlineData(ReactionKind.Parry, 1.00f, ReactionKind.Parry)]   // exactly on impact
    [InlineData(ReactionKind.Parry, 1.04f, ReactionKind.Parry)]   // inside the 100 ms parry window
    [InlineData(ReactionKind.Parry, 1.08f, ReactionKind.None)]    // outside it
    [InlineData(ReactionKind.Dodge, 0.92f, ReactionKind.Dodge)]   // inside the 200 ms dodge window
    [InlineData(ReactionKind.Dodge, 0.50f, ReactionKind.None)]    // way too early
    public void ReactionWindows(ReactionKind press, float at, ReactionKind expected)
    {
        var t = new ReactionTimer(1f, Config().Reactions);
        t.Advance(at);
        t.Press(press);
        Assert.True(t.Resolved);
        Assert.Equal(expected, t.Outcome);
    }

    [Fact]
    public void OnlyOnePressCountsAndTheHitWaitsForImpact()
    {
        var t = new ReactionTimer(1f, Config().Reactions);
        t.Advance(0.3f);
        t.Press(ReactionKind.Parry);                 // early: miss
        t.Advance(0.7f);
        t.Press(ReactionKind.Parry);                 // perfect, but too late to change it
        Assert.Equal(ReactionKind.None, t.Outcome);
        Assert.True(t.Done);
    }

    [Fact]
    public void NoPressMissesOnceTheWindowCloses()
    {
        var t = new ReactionTimer(1f, Config().Reactions);
        t.Advance(1.05f);
        Assert.False(t.Resolved);
        t.Advance(0.1f);
        Assert.True(t.Done);
        Assert.Equal(ReactionKind.None, t.Outcome);
    }

    [Fact]
    public void ParryAvoidsDamageGivesApAndCounters()
    {
        var p = Make("p", Team.Player, atk: 30, spd: 1);
        var e = Make("e", Team.Enemy, atk: 50, spd: 1000);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1), Config());
        Assert.Same(e, b.Current);
        int apBefore = p.Ap;

        var r = b.Execute(new AttackAction(), p, ReactionKind.Parry);

        Assert.Equal(100, p.Stats.Hp);
        Assert.Equal(apBefore + 1, p.Ap);
        Assert.NotNull(r.Counter);
        Assert.True(r.Counter.Damage > 0);
        Assert.Equal(100 - r.Counter.Damage, e.Stats.Hp);
    }

    [Fact]
    public void DodgeAvoidsDamageWithoutReward()
    {
        var p = Make("p", Team.Player, spd: 1);
        var e = Make("e", Team.Enemy, atk: 50, spd: 1000);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1), Config());
        int apBefore = p.Ap;

        var r = b.Execute(new AttackAction(), p, ReactionKind.Dodge);

        Assert.Equal(100, p.Stats.Hp);
        Assert.Equal(apBefore, p.Ap);
        Assert.Null(r.Counter);
        Assert.Equal(100, e.Stats.Hp);
    }

    [Fact]
    public void ReactionWithoutTheMechanicFailsLoud()
    {
        var p = Make("p", Team.Player, spd: 1);
        var e = Make("e", Team.Enemy, spd: 1000);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1));
        Assert.Throws<ArgumentException>(() => b.Execute(new AttackAction(), p, ReactionKind.Parry));
    }

    [Fact]
    public void DirectorWaitsForTheReactionAndAPerfectPlayerTakesNoDamage()
    {
        var game = new SoloGame();
        var ui = new PerfectParryUi();
        var d = new EncounterDirector(game, ui, new EncounterConfig { EnemyActionDelaySeconds = 0, Seed = 5, Battle = Config() });

        for (int i = 0; i < 20000 && game.Resumed == 0; i++) d.Tick(0.01f);

        Assert.Equal(1, game.Resumed);
        Assert.True(ui.Reactions > 0);
        Assert.Equal(game.Player.Stats.MaxHp, game.Player.Stats.Hp);
        Assert.False(game.Enemy.IsAlive);
    }

    [Fact]
    public void ReactionsNeedAReactionUi()
    {
        Assert.Throws<ArgumentException>(() =>
            new EncounterDirector(new SoloGame(), new MenuOnlyUi(), new EncounterConfig { Battle = Config() }));
    }

    private sealed class SoloGame : IGameAdapter
    {
        public readonly Combatant Player = Make("hero", Team.Player, hp: 100, atk: 15, spd: 100);
        public readonly Combatant Enemy = Make("brute", Team.Enemy, hp: 120, atk: 30, spd: 150);
        public int Resumed;

        public string GameId => "fake";
        public bool CanStartEncounter => Resumed == 0;
        public IReadOnlyList<Combatant> GetPlayerParty() => new[] { Player };
        public IReadOnlyList<Combatant> FindNearbyEnemies(float radius) => new[] { Enemy };
        public void FreezeWorld() { }
        public void ResumeWorld() => Resumed++;
        public void ApplyHp(Combatant c) { }
        public void Kill(Combatant c) { }
        public void PresentAction(ActionResult result) { }
        public void OnPlayerDefeated() { }
        public IReadOnlyList<IBattleAction> PlayerActions(Combatant player) => new IBattleAction[] { new AttackAction() };
    }

    private class MenuOnlyUi : IBattleUi
    {
        protected BattleEngine B;
        public void Open(BattleEngine battle) => B = battle;
        public void Close() { }
        public void ShowResult(ActionResult result) { }
        public bool TryGetPlayerChoice(Combatant actor, out IBattleAction action, out Combatant target)
        {
            action = new AttackAction();
            target = B.Enemies.First(e => e.IsAlive);
            return true;
        }
    }

    /// <summary>Presses Parry on the first frame that falls inside the parry window.</summary>
    private sealed class PerfectParryUi : MenuOnlyUi, IReactionUi
    {
        private ReactionTimer _t;
        public int Reactions;

        public void BeginReaction(Combatant attacker, Combatant target, ReactionTimer timer) { _t = timer; Reactions++; }

        public ReactionKind PollReaction() =>
            Math.Abs(_t.Elapsed - _t.ImpactAt) <= _t.ParryWindow / 2 ? ReactionKind.Parry : ReactionKind.None;

        public void EndReaction(ReactionKind outcome) => Assert.Equal(ReactionKind.Parry, outcome);
    }
}
