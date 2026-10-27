using System;
using System.Collections.Generic;
using System.Linq;
using AnyTurnBased.Core;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace AnyTurnBased.Smapi
{
    /// <summary>
    /// Stardew Valley 1.6. Member names below are **documented** (SMAPI/decompiled 1.6 API), not yet
    /// **observed** on this install: PLAYBOOK §3 confirms each one by log.
    /// </summary>
    internal sealed class StardewAdapter : IGameAdapter
    {
        private const float TileSize = 64f;
        private readonly IMonitor _monitor;

        public StardewAdapter(IMonitor monitor) { _monitor = monitor; }

        public string GameId => "stardew-valley";

        public bool CanStartEncounter =>
            Context.IsWorldReady && Context.IsPlayerFree && !Game1.eventUp && Game1.activeClickableMenu == null;

        public IReadOnlyList<Combatant> GetPlayerParty()
        {
            var f = Game1.player;
            var weapon = f.CurrentTool as MeleeWeapon
                ?? f.Items.OfType<MeleeWeapon>().FirstOrDefault();
            float dmg = weapon != null ? (weapon.minDamage.Value + weapon.maxDamage.Value) / 2f : 1f;

            var stats = StatMapper.FromNative(f.health, f.maxHealth, dmg, f.buffs.Defense,
                nativeMoveSpeed: f.getMovementSpeed(), referenceMoveSpeed: f.getMovementSpeed(),
                peerHp: f.maxHealth);
            stats.MaxMp = Math.Max(1, f.MaxStamina / 10);
            stats.Mp = Math.Max(0, (int)f.Stamina / 10);
            return new[] { new Combatant("player", f.displayName, Team.Player, stats, f) };
        }

        public IReadOnlyList<Combatant> FindNearbyEnemies(float radiusTiles)
        {
            var loc = Game1.currentLocation ?? throw new InvalidOperationException("[AnyTurnBased] no current location");
            var playerPos = Game1.player.Position;
            float playerSpeed = Game1.player.getMovementSpeed();
            int playerHp = Game1.player.maxHealth;

            return loc.characters.OfType<Monster>()
                .Where(m => m.Health > 0 && !m.IsInvisible)
                .Where(m => Vector2.Distance(m.Position, playerPos) <= radiusTiles * TileSize)
                .Take(4)
                .Select((m, i) => new Combatant($"monster{i}", m.displayName, Team.Enemy,
                    StatMapper.FromNative(m.Health, m.MaxHealth, m.DamageToFarmer, m.resilience.Value,
                        Math.Max(1, m.Speed), playerSpeed, playerHp), m))
                .ToList();
        }

        public void FreezeWorld()
        {
            // Monster.update is skipped by the Harmony prefix in ModEntry; this stops the player and time.
            Game1.player.CanMove = false;
            Game1.player.Halt();
            Game1.freezeControls = true;
        }

        public void ResumeWorld()
        {
            Game1.freezeControls = false;
            Game1.player.CanMove = true;
            // Brief invincibility so a monster touching the player does not hit right after resuming.
            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.currentTemporaryInvincibilityDuration = 1500;
        }

        public void ApplyHp(Combatant c)
        {
            switch (c.NativeHandle)
            {
                case Farmer f: f.health = Math.Max(1, c.Stats.Hp); break;
                case Monster m: m.Health = Math.Max(1, c.Stats.Hp); break;
                default: throw new InvalidOperationException($"[AnyTurnBased] unknown native handle for {c}");
            }
        }

        public void Kill(Combatant c)
        {
            if (!(c.NativeHandle is Monster m)) throw new InvalidOperationException($"[AnyTurnBased] {c} is not a Monster");
            var loc = Game1.currentLocation;
            // damageMonster is the vanilla path: it grants XP, rolls drops and counts slayer quests.
            var box = m.GetBoundingBox();
            loc.damageMonster(box, 99999, 99999, isBomb: false, Game1.player, isProjectile: true);
            if (loc.characters.Contains(m))
                _monitor.Log($"[AnyTurnBased] {m.Name} survived damageMonster; check docs/09", LogLevel.Warn);
        }

        public void PresentAction(ActionResult result)
        {
            if (result.Target?.NativeHandle is Character ch && result.Damage > 0)
                Game1.currentLocation.debris.Add(new Debris(result.Damage, ch.Position, Color.White, 1f, ch));
        }

        public void OnPlayerDefeated() => Game1.player.health = 0; // vanilla pass-out flow runs on next tick

        public IReadOnlyList<IBattleAction> PlayerActions(Combatant player) => new IBattleAction[]
        {
            new AttackAction(),
            new SkillAction("Golpe especial", TargetKind.SingleEnemy, mpCost: 4, power: 1.8f),
            new SkillAction("Comer", TargetKind.Self, mpCost: 2, power: 3f),
            new DefendAction(),
            new FleeAction(),
        };
    }
}
