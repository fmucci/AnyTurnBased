using System;
using System.Collections.Generic;
using System.Linq;
using AnyTurnBased.Core;
using Xunit;

public class BattleEngineTests
{
    private static Combatant Make(string name, Team team, int hp = 100, int atk = 20, int def = 0, int spd = 100) =>
        new Combatant(name, name, team, new Stats { MaxHp = hp, Hp = hp, Attack = atk, Defense = def, Speed = spd, MaxMp = 10, Mp = 10 }, null);

    [Fact]
    public void FasterCombatantActsMoreOften()
    {
        var fast = Make("fast", Team.Enemy, spd: 200);
        var slow = Make("slow", Team.Player, spd: 100);
        var b = new BattleEngine(new[] { slow, fast }, new SystemRandom(1));
        var order = b.PreviewOrder(6).ToList();
        Assert.Equal(4, order.Count(c => c == fast));
        Assert.Equal(2, order.Count(c => c == slow));
    }

    [Fact]
    public void KillingLastEnemyIsVictory()
    {
        var p = Make("p", Team.Player, atk: 1000, spd: 500);
        var e = Make("e", Team.Enemy, hp: 10);
        var b = new BattleEngine(new[] { p, e }, new SystemRandom(1));
        Assert.Same(p, b.Current);
        b.Execute(new AttackAction(), e);
        Assert.Equal(BattleOutcome.Victory, b.Outcome);
        Assert.False(e.IsAlive);
    }

    [Fact]
    public void DefendHalvesDamage()
    {
        var rng = new FixedRandom(0.5f);
        int normal = Damage.Compute(40, 0, 1f, false, rng);
        int defended = Damage.Compute(40, 0, 1f, true, rng);
        Assert.Equal(normal / 2, defended);
    }

    [Fact]
    public void AttackingAnAllyFailsLoud()
    {
        var p1 = Make("p1", Team.Player, spd: 500);
        var p2 = Make("p2", Team.Player);
        var e = Make("e", Team.Enemy);
        var b = new BattleEngine(new[] { p1, p2, e }, new SystemRandom(1));
        Assert.Throws<ArgumentException>(() => b.Execute(new AttackAction(), p2));
    }

    [Fact]
    public void DirectorRunsFullEncounterAndAppliesResult()
    {
        var game = new FakeGame();
        var ui = new AutoAttackUi();
        var d = new EncounterDirector(game, ui, new EncounterConfig { EnemyActionDelaySeconds = 0, Seed = 3 });

        for (int i = 0; i < 1000 && game.Resumed == 0; i++) d.Tick(0.016f);

        Assert.Equal(1, game.Frozen);
        Assert.Equal(1, game.Resumed);
        Assert.Contains(game.Enemy, game.Killed);
        Assert.Equal(DirectorState.Cooldown, d.State);
    }

    private sealed class FixedRandom : IRandom
    {
        private readonly float _v;
        public FixedRandom(float v) { _v = v; }
        public float NextFloat() => _v;
    }

    private sealed class FakeGame : IGameAdapter
    {
        public readonly Combatant Player = Make("hero", Team.Player, hp: 200, atk: 40);
        public readonly Combatant Enemy = Make("slime", Team.Enemy, hp: 60, atk: 5);
        public int Frozen, Resumed;
        public readonly List<Combatant> Killed = new List<Combatant>();

        public string GameId => "fake";
        public bool CanStartEncounter => true;
        public IReadOnlyList<Combatant> GetPlayerParty() => new[] { Player };
        public IReadOnlyList<Combatant> FindNearbyEnemies(float radius) => Enemy.IsAlive ? new[] { Enemy } : Array.Empty<Combatant>();
        public void FreezeWorld() => Frozen++;
        public void ResumeWorld() => Resumed++;
        public void ApplyHp(Combatant c) { }
        public void Kill(Combatant c) => Killed.Add(c);
        public void PresentAction(ActionResult result) { }
        public void OnPlayerDefeated() { }
        public IReadOnlyList<IBattleAction> PlayerActions(Combatant player) => new IBattleAction[] { new AttackAction() };
    }

    private sealed class AutoAttackUi : IBattleUi
    {
        private BattleEngine _b;
        public void Open(BattleEngine battle) => _b = battle;
        public void Close() { }
        public void ShowResult(ActionResult result) { }
        public bool TryGetPlayerChoice(Combatant actor, out IBattleAction action, out Combatant target)
        {
            action = new AttackAction();
            target = _b.Enemies.First(e => e.IsAlive);
            return true;
        }
    }
}
