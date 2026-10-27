using System;

namespace AnyTurnBased.Core
{
    public enum Team { Player, Enemy }

    public sealed class Stats
    {
        public int MaxHp;
        public int Hp;
        public int MaxMp;
        public int Mp;
        public int Attack;
        public int Defense;
        public int Speed;

        public Stats Clone() => (Stats)MemberwiseClone();
    }

    /// <summary>A participant in a battle. NativeHandle points back at the game's own entity.</summary>
    public sealed class Combatant
    {
        public string Id { get; }
        public string Name { get; }
        public Team Team { get; }
        public Stats Stats { get; }
        public object NativeHandle { get; }

        /// <summary>Hp when the battle started; the adapter applies the difference.</summary>
        public int StartHp { get; }
        public bool Defending { get; internal set; }

        /// <summary>Action points (economy.ap). Stays 0 when the mechanic is off.</summary>
        public int Ap { get; internal set; }

        /// <summary>CTB counter: lowest value acts next.</summary>
        internal long NextTurnAt;

        public bool IsAlive => Stats.Hp > 0;

        public Combatant(string id, string name, Team team, Stats stats, object nativeHandle)
        {
            if (stats.Speed <= 0) throw new ArgumentException($"{name}: Speed must be > 0 (got {stats.Speed})");
            if (stats.MaxHp <= 0) throw new ArgumentException($"{name}: MaxHp must be > 0 (got {stats.MaxHp})");
            Id = id;
            Name = name;
            Team = team;
            Stats = stats;
            NativeHandle = nativeHandle;
            StartHp = stats.Hp;
        }

        public override string ToString() => $"{Name} [{Team}] {Stats.Hp}/{Stats.MaxHp}";
    }
}
