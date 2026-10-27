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
    /// so a fast enemy can act twice before a slow one. Optional mechanics come from <see cref="BattleConfig"/>.
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
        public BattleConfig Config { get; }

        public BattleEngine(IEnumerable<Combatant> combatants, IRandom rng, BattleConfig config = null)
        {
            _all = combatants.ToList();
            _rng = rng;
            Config = config ?? BattleConfig.Default;
            if (!Players.Any()) throw new ArgumentException("battle needs at least one player combatant");
            if (!Enemies.Any()) throw new ArgumentException("battle needs at least one enemy combatant");
            foreach (var c in _all) c.NextTurnAt = TurnCost / c.Stats.Speed;
            if (Config.Ap != null) foreach (var p in Players) p.Ap = Config.Ap.Start;
            Advance();
        }

        /// <summary>MP always; AP only for players when economy.ap is on. The UI greys out what this rejects.</summary>
        public bool CanAfford(Combatant actor, IBattleAction action) =>
            actor.Stats.Mp >= action.MpCost &&
            (Config.Ap == null || actor.Team != Team.Player || actor.Ap >= action.ApCost);

        /// <summary>True when the player gets a dodge/parry window against this enemy action.</summary>
        public bool IsReactable(IBattleAction action, Combatant target) =>
            Config.Reactions != null && Current.Team == Team.Enemy &&
            action.Targets == TargetKind.SingleEnemy && target != null && target.Team == Team.Player;

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

        /// <param name="reaction">How the targeted player reacted (reactive.dodge-parry); None when there was no window or it was missed.</param>
        public ActionResult Execute(IBattleAction action, Combatant target, ReactionKind reaction = ReactionKind.None)
        {
            if (Outcome != BattleOutcome.Ongoing) throw new InvalidOperationException($"battle already ended: {Outcome}");
            if (action.Targets == TargetKind.SingleEnemy && (target == null || target.Team == Current.Team || !target.IsAlive))
                throw new ArgumentException($"{action.Name} needs a living opponent, got {target}");
            if (action.Targets == TargetKind.SingleAlly && (target == null || target.Team != Current.Team || !target.IsAlive))
                throw new ArgumentException($"{action.Name} needs a living ally, got {target}");
            if (reaction != ReactionKind.None && !IsReactable(action, target))
                throw new ArgumentException($"{reaction} is not possible against {action.Name} on {target}");

            var actor = Current;
            bool usesAp = Config.Ap != null && actor.Team == Team.Player;
            if (usesAp)
            {
                if (actor.Ap < action.ApCost) throw new InvalidOperationException($"{actor.Name} lacks AP for {action.Name} ({actor.Ap}/{action.ApCost})");
                actor.Ap -= action.ApCost;
            }

            actor.Defending = false;
            var result = reaction == ReactionKind.None
                ? action.Resolve(actor, action.Targets == TargetKind.Self ? actor : target, _rng)
                : Avoid(action, target, reaction);
            _log.Add(result);
            if (result.Counter != null) _log.Add(result.Counter);
            if (usesAp && action is AttackAction) GainAp(actor, Config.Ap.OnBasicAttack);

            if (result.Fled) Outcome = BattleOutcome.Fled;
            else if (!Enemies.Any(e => e.IsAlive)) Outcome = BattleOutcome.Victory;
            else if (!Players.Any(p => p.IsAlive)) Outcome = BattleOutcome.Defeat;

            actor.NextTurnAt += TurnCost / actor.Stats.Speed;
            if (Outcome == BattleOutcome.Ongoing) Advance();
            return result;
        }

        /// <summary>The hit never lands. A parry also gives AP and, if enabled, a free basic attack back.</summary>
        private ActionResult Avoid(IBattleAction action, Combatant target, ReactionKind reaction)
        {
            var attacker = Current;
            var r = new ActionResult { Actor = attacker, Target = target, Action = action, Reaction = reaction };
            if (reaction == ReactionKind.Dodge)
            {
                r.Message = $"{target.Name} esquiva {action.Name} de {attacker.Name}";
                return r;
            }

            r.Message = $"{target.Name} para {action.Name} de {attacker.Name}";
            if (Config.Ap != null) GainAp(target, Config.Ap.OnPerfectReaction);
            if (Config.Reactions.CounterOnParry)
            {
                r.Counter = new AttackAction().Resolve(target, attacker, _rng);
                r.Counter.Message = $"{target.Name} contraataca a {attacker.Name}: {r.Counter.Damage}";
                r.Message += "; " + r.Counter.Message;
            }
            return r;
        }

        private void GainAp(Combatant c, int amount) => c.Ap = Math.Min(Config.Ap.Max, c.Ap + amount);

        private void Advance()
        {
            Current = _all.Where(c => c.IsAlive).OrderBy(c => c.NextTurnAt).ThenBy(c => c.Team).First();
            if (Config.Ap != null && Current.Team == Team.Player) GainAp(Current, Config.Ap.PerTurn);
        }
    }
}
