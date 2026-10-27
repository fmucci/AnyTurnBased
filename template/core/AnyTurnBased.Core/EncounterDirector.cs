using System;
using System.Linq;

namespace AnyTurnBased.Core
{
    public enum DirectorState { Exploring, InBattle, Cooldown }

    public sealed class EncounterConfig
    {
        public float EncounterRadius = 5f;
        public float CooldownSeconds = 3f;
        /// <summary>Pause between enemy actions so the player can read them.</summary>
        public float EnemyActionDelaySeconds = 0.8f;
        public int Seed = Environment.TickCount;
        /// <summary>Mechanics from game/battle.json (BattleConfig.FromJson); ctb-classic when the game has none.</summary>
        public BattleConfig Battle = BattleConfig.Default;
    }

    /// <summary>
    /// The universal loop: explore → detect → freeze → turn-based battle → apply → resume.
    /// The adapter calls Tick() once per frame with real (unscaled) delta time.
    /// </summary>
    public sealed class EncounterDirector
    {
        private readonly IGameAdapter _game;
        private readonly IBattleUi _ui;
        private readonly EncounterConfig _cfg;
        private readonly IRandom _rng;
        private readonly IEnemyAi _ai;
        private float _timer;

        // Enemy action waiting for the player's dodge/parry (reactive.dodge-parry).
        private ReactionTimer _reaction;
        private IBattleAction _pendingAction;
        private Combatant _pendingTarget;

        public DirectorState State { get; private set; } = DirectorState.Exploring;
        public BattleEngine Battle { get; private set; }
        public Action<string> Log = _ => { };

        public EncounterDirector(IGameAdapter game, IBattleUi ui, EncounterConfig cfg, IEnemyAi ai = null)
        {
            if (cfg.Battle.Reactions != null && !(ui is IReactionUi))
                throw new ArgumentException($"reactive.dodge-parry is on but {ui.GetType().Name} does not implement IReactionUi");
            _game = game;
            _ui = ui;
            _cfg = cfg;
            _rng = new SystemRandom(cfg.Seed);
            _ai = ai ?? new SimpleEnemyAi(_rng);
        }

        public void Tick(float unscaledDelta)
        {
            switch (State)
            {
                case DirectorState.Exploring: TryStart(); break;
                case DirectorState.InBattle: Step(unscaledDelta); break;
                case DirectorState.Cooldown:
                    _timer -= unscaledDelta;
                    if (_timer <= 0) State = DirectorState.Exploring;
                    break;
            }
        }

        /// <summary>Debug hook used by PLAYBOOK §5 to force a battle.</summary>
        public bool TryStart()
        {
            if (State != DirectorState.Exploring || !_game.CanStartEncounter) return false;
            var enemies = _game.FindNearbyEnemies(_cfg.EncounterRadius);
            if (enemies.Count == 0) return false;

            var party = _game.GetPlayerParty();
            _game.FreezeWorld();
            Battle = new BattleEngine(party.Concat(enemies), _rng, _cfg.Battle);
            Log($"[AnyTurnBased] encounter: {string.Join(", ", Battle.All)}");
            if (_cfg.Battle.Unsupported.Count > 0)
                Log($"[AnyTurnBased] not in the core yet, ignored: {string.Join(", ", _cfg.Battle.Unsupported)}");
            _ui.Open(Battle);
            _timer = 0;
            State = DirectorState.InBattle;
            return true;
        }

        private void Step(float dt)
        {
            if (Battle.Outcome != BattleOutcome.Ongoing) { Finish(); return; }
            if (_reaction != null) { StepReaction(dt); return; }

            var actor = Battle.Current;
            IBattleAction action;
            Combatant target;
            if (actor.Team == Team.Player)
            {
                if (!_ui.TryGetPlayerChoice(actor, out action, out target)) return;
            }
            else
            {
                _timer += dt;
                if (_timer < _cfg.EnemyActionDelaySeconds) return;
                _timer = 0;
                (action, target) = _ai.Choose(Battle, actor);
                if (Battle.IsReactable(action, target))
                {
                    var rules = _cfg.Battle.Reactions;
                    float lead = rules.MinLead + (rules.MaxLead - rules.MinLead) * _rng.NextFloat();
                    _reaction = new ReactionTimer(lead, rules);
                    _pendingAction = action;
                    _pendingTarget = target;
                    ((IReactionUi)_ui).BeginReaction(actor, target, _reaction);
                    return;
                }
            }

            Run(action, target, ReactionKind.None);
        }

        private void StepReaction(float dt)
        {
            var ui = (IReactionUi)_ui;
            _reaction.Press(ui.PollReaction());
            _reaction.Advance(dt);
            if (!_reaction.Done) return;

            var outcome = _reaction.Outcome;
            ui.EndReaction(outcome);
            var action = _pendingAction;
            var target = _pendingTarget;
            _reaction = null;
            _pendingAction = null;
            _pendingTarget = null;
            Run(action, target, outcome);
        }

        private void Run(IBattleAction action, Combatant target, ReactionKind reaction)
        {
            var result = Battle.Execute(action, target, reaction);
            Log("[AnyTurnBased] " + result.Message);
            _ui.ShowResult(result);
            _game.PresentAction(result);
        }

        private void Finish()
        {
            Log($"[AnyTurnBased] battle ended: {Battle.Outcome}");
            _ui.Close();
            foreach (var c in Battle.All)
            {
                if (c.Stats.Hp != c.StartHp) _game.ApplyHp(c);
                if (c.Team == Team.Enemy && !c.IsAlive) _game.Kill(c);
            }
            _game.ResumeWorld();
            if (Battle.Outcome == BattleOutcome.Defeat) _game.OnPlayerDefeated();
            Battle = null;
            _timer = _cfg.CooldownSeconds;
            State = DirectorState.Cooldown;
        }
    }
}
