using System;
using System.Collections.Generic;
using System.Linq;
using AnyTurnBased.Core;
using BepInEx;
using UnityEngine;

namespace AnyTurnBased.Unity
{
    [BepInPlugin("fmucci.anyturnbased", "AnyTurnBased", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private EncounterDirector _director;
        private ImguiBattleUi _ui;

        private void Awake()
        {
            var adapter = new UnityAdapter();
            _ui = new ImguiBattleUi(adapter);
            _director = new EncounterDirector(adapter, _ui, new EncounterConfig { EncounterRadius = 8f })
            {
                Log = Logger.LogInfo,
            };
            Logger.LogInfo($"[AnyTurnBased] loaded {Application.productName} {Application.version}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9)) _director.TryStart();
            _director.Tick(Time.unscaledDeltaTime);
        }

        private void OnGUI() => _ui.Draw();
    }

    /// <summary>
    /// GAME-SPECIFIC: replace TODO members with the game's classes (found with dnSpy / Il2CppDumper).
    /// Everything left unimplemented throws, so the first run says exactly what is missing.
    /// </summary>
    internal sealed class UnityAdapter : IGameAdapter
    {
        private float _savedTimeScale = 1f;

        public string GameId => Application.productName;
        public bool CanStartEncounter => Time.timeScale > 0f; // TODO: also exclude menus/cutscenes

        public IReadOnlyList<Combatant> GetPlayerParty() =>
            throw new NotImplementedException("[AnyTurnBased] GetPlayerParty: find the player component (docs/03)");

        public IReadOnlyList<Combatant> FindNearbyEnemies(float radius) =>
            throw new NotImplementedException("[AnyTurnBased] FindNearbyEnemies: find the enemy component (docs/03)");

        public void FreezeWorld()
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        public void ResumeWorld() => Time.timeScale = _savedTimeScale;

        public void ApplyHp(Combatant c) =>
            throw new NotImplementedException($"[AnyTurnBased] ApplyHp: write HP for {c} via the game's damage method");

        public void Kill(Combatant c) =>
            throw new NotImplementedException($"[AnyTurnBased] Kill: route {c} through the game's death method");

        public void PresentAction(ActionResult result) { }
        public void OnPlayerDefeated() { }

        public IReadOnlyList<IBattleAction> PlayerActions(Combatant player) => new IBattleAction[]
        {
            new AttackAction(), new SkillAction("Especial", TargetKind.SingleEnemy, 5, 1.8f),
            new DefendAction(), new FleeAction(),
        };
    }

    /// <summary>IMGUI menu: independent of the game's UI and unaffected by timeScale.</summary>
    internal sealed class ImguiBattleUi : IBattleUi
    {
        private readonly IGameAdapter _game;
        private BattleEngine _battle;
        private IReadOnlyList<IBattleAction> _actions;
        private IBattleAction _picked;
        private IBattleAction _readyAction;
        private Combatant _readyTarget;
        private string _message = "";

        public ImguiBattleUi(IGameAdapter game) { _game = game; }

        public void Open(BattleEngine battle) { _battle = battle; _actions = _game.PlayerActions(battle.Players.First()); }
        public void Close() => _battle = null;
        public void ShowResult(ActionResult result) => _message = result.Message;

        public bool TryGetPlayerChoice(Combatant actor, out IBattleAction action, out Combatant target)
        {
            action = _readyAction;
            target = _readyTarget;
            if (action == null) return false;
            _readyAction = null;
            _readyTarget = null;
            return true;
        }

        public void Draw()
        {
            if (_battle == null) return;
            GUILayout.BeginArea(new Rect(20, Screen.height - 320, 560, 300), GUI.skin.box);
            foreach (var c in _battle.All)
                GUILayout.Label($"{(c == _battle.Current ? "> " : "  ")}{c.Name}  {c.Stats.Hp}/{c.Stats.MaxHp}");
            GUILayout.Label(_message);

            if (_battle.Current.Team == Team.Player && _readyAction == null)
            {
                if (_picked == null)
                {
                    foreach (var a in _actions)
                        if (GUILayout.Button(a.Name) && a.MpCost <= _battle.Current.Stats.Mp)
                        {
                            if (a.Targets == TargetKind.SingleEnemy) _picked = a;
                            else _readyAction = a;
                        }
                }
                else
                {
                    foreach (var e in _battle.Enemies.Where(e => e.IsAlive))
                        if (GUILayout.Button(e.Name)) { _readyAction = _picked; _readyTarget = e; _picked = null; }
                    if (GUILayout.Button("Volver")) _picked = null;
                }
            }
            GUILayout.EndArea();
        }
    }
}
