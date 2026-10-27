using System.Linq;

namespace AnyTurnBased.Core
{
    public interface IEnemyAi
    {
        (IBattleAction action, Combatant target) Choose(BattleEngine battle, Combatant self);
    }

    /// <summary>Hits the weakest living player; defends when below 25% HP one time in three.</summary>
    public sealed class SimpleEnemyAi : IEnemyAi
    {
        private static readonly AttackAction Attack = new AttackAction();
        private static readonly DefendAction Defend = new DefendAction();
        private readonly IRandom _rng;

        public SimpleEnemyAi(IRandom rng) { _rng = rng; }

        public (IBattleAction action, Combatant target) Choose(BattleEngine battle, Combatant self)
        {
            if (self.Stats.Hp * 4 < self.Stats.MaxHp && _rng.NextFloat() < 0.33f)
                return (Defend, self);
            var target = battle.Players.Where(p => p.IsAlive).OrderBy(p => p.Stats.Hp).First();
            return (Attack, target);
        }
    }
}
