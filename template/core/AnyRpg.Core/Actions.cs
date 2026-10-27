using System;

namespace AnyRpg.Core
{
    public enum TargetKind { Self, SingleEnemy, SingleAlly, None }

    public sealed class ActionResult
    {
        public Combatant Actor;
        public Combatant Target;
        public IBattleAction Action;
        public int Damage;
        public int Healed;
        public bool Fled;
        public string Message;
    }

    public interface IBattleAction
    {
        string Name { get; }
        TargetKind Targets { get; }
        int MpCost { get; }
        ActionResult Resolve(Combatant actor, Combatant target, IRandom rng);
    }

    public static class Damage
    {
        /// <summary>Attack * power, reduced by defense, +-10% variance, minimum 1.</summary>
        public static int Compute(int attack, int defense, float power, bool defending, IRandom rng)
        {
            float raw = attack * power - defense * 0.5f;
            float variance = 0.9f + 0.2f * rng.NextFloat();
            int dmg = Math.Max(1, (int)Math.Round(raw * variance));
            return defending ? Math.Max(1, dmg / 2) : dmg;
        }
    }

    public sealed class AttackAction : IBattleAction
    {
        public string Name => "Atacar";
        public TargetKind Targets => TargetKind.SingleEnemy;
        public int MpCost => 0;

        public ActionResult Resolve(Combatant actor, Combatant target, IRandom rng)
        {
            int dmg = Damage.Compute(actor.Stats.Attack, target.Stats.Defense, 1f, target.Defending, rng);
            target.Stats.Hp = Math.Max(0, target.Stats.Hp - dmg);
            return new ActionResult { Actor = actor, Target = target, Action = this, Damage = dmg,
                Message = $"{actor.Name} ataca a {target.Name}: {dmg}" };
        }
    }

    public sealed class DefendAction : IBattleAction
    {
        public string Name => "Defender";
        public TargetKind Targets => TargetKind.Self;
        public int MpCost => 0;

        public ActionResult Resolve(Combatant actor, Combatant target, IRandom rng)
        {
            actor.Defending = true;
            return new ActionResult { Actor = actor, Target = actor, Action = this,
                Message = $"{actor.Name} se defiende" };
        }
    }

    /// <summary>Generic skill. Adapters register game-flavoured ones (a spell, a weapon special).</summary>
    public sealed class SkillAction : IBattleAction
    {
        public string Name { get; }
        public TargetKind Targets { get; }
        public int MpCost { get; }
        public float Power { get; }

        public SkillAction(string name, TargetKind targets, int mpCost, float power)
        {
            Name = name; Targets = targets; MpCost = mpCost; Power = power;
        }

        public ActionResult Resolve(Combatant actor, Combatant target, IRandom rng)
        {
            if (actor.Stats.Mp < MpCost) throw new InvalidOperationException($"{actor.Name} lacks MP for {Name}");
            actor.Stats.Mp -= MpCost;
            var r = new ActionResult { Actor = actor, Target = target, Action = this };
            if (Targets == TargetKind.SingleAlly || Targets == TargetKind.Self)
            {
                int heal = Math.Max(1, (int)(actor.Stats.MaxHp * 0.1f * Power));
                int before = target.Stats.Hp;
                target.Stats.Hp = Math.Min(target.Stats.MaxHp, target.Stats.Hp + heal);
                r.Healed = target.Stats.Hp - before;
                r.Message = $"{actor.Name} usa {Name} en {target.Name}: +{r.Healed}";
            }
            else
            {
                r.Damage = Damage.Compute(actor.Stats.Attack, target.Stats.Defense, Power, target.Defending, rng);
                target.Stats.Hp = Math.Max(0, target.Stats.Hp - r.Damage);
                r.Message = $"{actor.Name} usa {Name} en {target.Name}: {r.Damage}";
            }
            return r;
        }
    }

    public sealed class FleeAction : IBattleAction
    {
        public string Name => "Huir";
        public TargetKind Targets => TargetKind.None;
        public int MpCost => 0;

        public ActionResult Resolve(Combatant actor, Combatant target, IRandom rng)
        {
            bool ok = rng.NextFloat() < 0.6f;
            return new ActionResult { Actor = actor, Action = this, Fled = ok,
                Message = ok ? $"{actor.Name} huye" : $"{actor.Name} no pudo huir" };
        }
    }
}
