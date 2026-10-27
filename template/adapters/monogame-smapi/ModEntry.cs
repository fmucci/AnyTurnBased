using AnyRpg.Core;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace AnyRpg.Smapi
{
    public sealed class ModEntry : Mod
    {
        internal static EncounterDirector Director;

        private StardewAdapter _adapter;
        private StardewBattleUi _ui;

        public override void Entry(IModHelper helper)
        {
            _adapter = new StardewAdapter(Monitor);
            _ui = new StardewBattleUi(_adapter);
            Director = new EncounterDirector(_adapter, _ui, new EncounterConfig { EncounterRadius = 4f })
            {
                Log = msg => Monitor.Log(msg, LogLevel.Info),
            };

            // Monsters keep moving unless their update is skipped while a battle runs (docs/07).
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                AccessTools.Method(typeof(Monster), nameof(Monster.update), new[] { typeof(Microsoft.Xna.Framework.GameTime), typeof(GameLocation) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(SkipWhileFrozen)));

            helper.Events.GameLoop.UpdateTicked += (_, e) => Director.Tick(1f / 60f);
            helper.Events.Display.RenderedHud += (_, e) => _ui.Draw(e.SpriteBatch);
            helper.Events.Input.ButtonPressed += (_, e) => _ui.OnButton(e.Button, helper.Input);

            helper.ConsoleCommands.Add("anyrpg_battle", "Fuerza un encuentro con el monstruo más cercano.",
                (_, _) => Monitor.Log(Director.TryStart() ? "battle started" : "no enemy in range", LogLevel.Info));

            Monitor.Log($"[AnyRPG] loaded {_adapter.GameId} {Game1.version}", LogLevel.Info);
        }

        private static bool SkipWhileFrozen() => Director?.State != DirectorState.InBattle;
    }
}
