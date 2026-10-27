using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AnyTurnBased.Core
{
    /// <summary>Rules for economy.ap (Expedition 33 style action points). Only player combatants use AP.</summary>
    public sealed class ApRules
    {
        public int Start = 1;
        public int Max = 9;
        public int PerTurn = 1;
        public int OnBasicAttack = 1;
        public int OnPerfectReaction = 1;
    }

    /// <summary>Rules for reactive.dodge-parry. Windows are total widths centred on the moment of impact.</summary>
    public sealed class ReactionRules
    {
        public float DodgeWindow = 0.18f;
        public float ParryWindow = 0.09f;
        public bool CounterOnParry = true;
        /// <summary>Time from the start of the enemy wind-up to the impact; random in this range so it can't be memorised.</summary>
        public float MinLead = 0.6f;
        public float MaxLead = 1.4f;
    }

    /// <summary>
    /// game/battle.json: the mechanics picked for a game (docs/13, mechanics/catalog.json).
    /// Mechanics the core does not implement yet are listed in <see cref="Unsupported"/>, never silently faked.
    /// </summary>
    public sealed class BattleConfig
    {
        /// <summary>Ids from the catalog that the core (or the adapter contract) honours today.</summary>
        public static readonly IReadOnlyCollection<string> Implemented = new HashSet<string>
        {
            "time.ctb",
            "economy.one-action", "economy.mp", "economy.ap",
            "actions.menu", "actions.items", "actions.defend", "actions.flee",
            "reactive.dodge-parry",
            "ai.hidden", "ai.turn-order-preview",
            "party.solo", "party.multi",
            "encounter.proximity", "encounter.manual",
            "victory.defeat-all",
        };

        private readonly HashSet<string> _mechanics;
        private readonly Dictionary<string, Dictionary<string, object>> _params;

        public string Time { get; }
        public string Preset { get; }
        public IReadOnlyCollection<string> Mechanics => _mechanics;
        public IReadOnlyList<string> Unsupported { get; }

        /// <summary>Null when economy.ap is off.</summary>
        public ApRules Ap { get; }

        /// <summary>Null when reactive.dodge-parry is off.</summary>
        public ReactionRules Reactions { get; }

        /// <summary>The ctb-classic preset: what the core did before battle.json existed.</summary>
        public static BattleConfig Default { get; } = new BattleConfig("time.ctb", "ctb-classic",
            new[] { "economy.one-action", "economy.mp", "actions.menu", "actions.defend", "actions.flee",
                    "ai.turn-order-preview", "party.solo", "encounter.proximity", "encounter.manual", "victory.defeat-all" },
            new Dictionary<string, Dictionary<string, object>>());

        public BattleConfig(string time, string preset, IEnumerable<string> mechanics,
            Dictionary<string, Dictionary<string, object>> parameters)
        {
            if (time != "time.ctb")
                throw new NotSupportedException($"battle.json: time model '{time}' is not implemented yet; the core only has time.ctb (docs/13)");
            Time = time;
            Preset = preset;
            _mechanics = new HashSet<string>(mechanics);
            _params = parameters;
            Unsupported = _mechanics.Where(m => !Implemented.Contains(m)).OrderBy(m => m).ToList();

            if (Has("economy.ap"))
                Ap = new ApRules
                {
                    Start = Int("economy.ap", "start", 1),
                    Max = Int("economy.ap", "max", 9),
                    PerTurn = Int("economy.ap", "perTurn", 1),
                    OnBasicAttack = Int("economy.ap", "onBasicAttack", 1),
                    OnPerfectReaction = Int("economy.ap", "onPerfectReaction", 1),
                };
            if (Has("reactive.dodge-parry"))
                Reactions = new ReactionRules
                {
                    DodgeWindow = Int("reactive.dodge-parry", "windowMs", 180) / 1000f,
                    ParryWindow = Int("reactive.dodge-parry", "parryWindowMs", 90) / 1000f,
                    CounterOnParry = Bool("reactive.dodge-parry", "counterOnFullParry", true),
                };
        }

        public static BattleConfig FromJson(string json)
        {
            if (!(MiniJson.Parse(json) is Dictionary<string, object> root))
                throw new FormatException("battle.json: root must be an object");
            if (!root.TryGetValue("version", out var v) || !(v is double d) || d != 1)
                throw new FormatException("battle.json: version must be 1");
            if (!root.TryGetValue("time", out var t) || !(t is string time))
                throw new FormatException("battle.json: missing 'time'");
            if (!root.TryGetValue("mechanics", out var m) || !(m is List<object> list) || list.Any(x => !(x is string)))
                throw new FormatException("battle.json: 'mechanics' must be a list of ids");

            var parameters = new Dictionary<string, Dictionary<string, object>>();
            if (root.TryGetValue("params", out var p) && p != null)
            {
                if (!(p is Dictionary<string, object> pd)) throw new FormatException("battle.json: 'params' must be an object");
                foreach (var kv in pd)
                    parameters[kv.Key] = kv.Value as Dictionary<string, object>
                        ?? throw new FormatException($"battle.json: params.{kv.Key} must be an object");
            }
            root.TryGetValue("preset", out var preset);
            return new BattleConfig(time, preset as string, list.Cast<string>(), parameters);
        }

        public bool Has(string mechanic) => mechanic == Time || _mechanics.Contains(mechanic);

        public int Int(string mechanic, string key, int fallback)
        {
            var v = Raw(mechanic, key);
            if (v == null) return fallback;
            if (v is double d && d == Math.Floor(d)) return (int)d;
            throw new FormatException($"battle.json: params.{mechanic}.{key} must be an integer, got {Describe(v)}");
        }

        public float Float(string mechanic, string key, float fallback)
        {
            var v = Raw(mechanic, key);
            if (v == null) return fallback;
            if (v is double d) return (float)d;
            throw new FormatException($"battle.json: params.{mechanic}.{key} must be a number, got {Describe(v)}");
        }

        public bool Bool(string mechanic, string key, bool fallback)
        {
            var v = Raw(mechanic, key);
            if (v == null) return fallback;
            if (v is bool b) return b;
            throw new FormatException($"battle.json: params.{mechanic}.{key} must be true or false, got {Describe(v)}");
        }

        private object Raw(string mechanic, string key) =>
            _params.TryGetValue(mechanic, out var p) && p.TryGetValue(key, out var v) ? v : null;

        private static string Describe(object v) => Convert.ToString(v, CultureInfo.InvariantCulture);
    }
}
