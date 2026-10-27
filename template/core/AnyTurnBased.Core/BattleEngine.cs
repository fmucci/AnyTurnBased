using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyTurnBased.Core
{
    public interface IRandom { float NextFloat(); }

    public sealed class SystemRandom : IRandom
    {
        private readonly Random _r;
        public SystemRandom(int seed) { _r = new Random(seed); }
        public float NextFloat() => (float)_r.NextDouble();
    }

    public enum BattleOutcome { Ongoing, Victory, Defeat, Fled }

    /// <summary>
    /// Conditional Turn Battle: each combatant waits 10000/Speed ticks between turns,
    /// so a fast enemy can act twice before a slow one.
    /// </summary>
    public sealed class BattleEngine
    {
        private const long TurnCost = 10000;

        private readonly List<Combatant> _all;
        private readonly IRandom _rng;
        private readonly List<ActionResult> _log = new List<ActionResult>();

        public IReadOnlyList<Combatant> All => _all;
        public IEnumerable<Combatant> Players => _all.Where(c => c.Team == Team.Player);
        public IEnumerable<Combatant> Enemies => _all.Where(c => c.Team == Team.Enemy);
        public IReadOnlyList<ActionResult> Log => _log;
        public BattleOutcome Outcome { get; private set; } = BattleOutcome.Ongoing;
        public Combatant Current { get; private set; }

        public BattleEngine(IEnumerable<Combatant> combatants, IRandom rng)
        {
            _all = combatants.ToList();
            _rng = rng;
            if (!Players.Any()) throw new ArgumentException("battle needs at least one player combatant");
            if (!Enemies.Any()) throw new ArgumentException("battle needs at least one enemy combatant");
            foreach (var c in _all) c.NextTurnAt = TurnCost / c.Stats.Speed;
            Advance();
        }

        /// <summary>Upcoming actors, for a turn-order bar in the UI.</summary>
        public IEnumerable<Combatant> PreviewOrder(int count)
        {
            var sim = _all.Where(c => c.IsAlive).ToDictionary(c => c, c => c.NextTurnAt);
            for (int i = 0; i < count && sim.Count > 0; i++)
            {
                var next = sim.OrderBy(kv => kv.Value).First().Key;
                yield return next;
                sim[next] += TurnCost / next.Stats.Speed;
            }
        }

        public ActionResult Execute(IBattleAction action, Combatant target)
        {
            if (Outcome != BattleOutcome.Ongoing) throw new InvalidOperationException($"battle already ended: {Outcome}");
            if (action.Targets == TargetKind.SingleEnemy && (target == null || target.Team == Current.Team || !target.IsAlive))
                throw new ArgumentException($"{action.Name} needs a living opponent, got {target}");
            if (action.Targets == TargetKind.SingleAlly && (target == null || target.Team != Current.Team || !target.IsAlive))
                throw new ArgumentException($"{action.Name} needs a living ally, got {target}");

            Current.Defending = false;
            var result = action.Resolve(Current, action.Targets == TargetKind.Self ? Current : target, _rng);
            _log.Add(result);

            if (result.Fled) Outcome = BattleOutcome.Fled;
            else if (!Enemies.Any(e => e.IsAlive)) Outcome = BattleOutcome.Victory;
            else if (!Players.Any(p => p.IsAlive)) Outcome = BattleOutcome.Defeat;

            Current.NextTurnAt += TurnCost / Current.Stats.Speed;
            if (Outcome == BattleOutcome.Ongoing) Advance();
            return result;
        }

        private void Advance()
        {
            Current = _all.Where(c => c.IsAlive).OrderBy(c => c.NextTurnAt).ThenBy(c => c.Team).First();
        }
    }
}
