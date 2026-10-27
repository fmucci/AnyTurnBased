using System.Collections.Generic;

namespace AnyTurnBased.Core
{
    /// <summary>
    /// Everything engine-specific. One implementation per engine/game (see PLAYBOOK §3).
    /// Implementations must fail loud: throw with a message naming what was not found.
    /// </summary>
    public interface IGameAdapter
    {
        string GameId { get; }

        /// <summary>False during menus, cutscenes, loading or dialogue: no encounters there.</summary>
        bool CanStartEncounter { get; }

        IReadOnlyList<Combatant> GetPlayerParty();

        /// <summary>Hostile entities within the encounter radius. Empty when none.</summary>
        IReadOnlyList<Combatant> FindNearbyEnemies(float radius);

        void FreezeWorld();
        void ResumeWorld();

        /// <summary>Write the new HP back to the native entity using the game's own damage path.</summary>
        void ApplyHp(Combatant c);

        /// <summary>Kill through the game's own death path so drops, XP and quests still fire.</summary>
        void Kill(Combatant c);

        /// <summary>Optional: play an attack/hit animation. Default adapters do nothing.</summary>
        void PresentAction(ActionResult result);

        /// <summary>Called on Defeat. Usually routes into the game's normal player-death flow.</summary>
        void OnPlayerDefeated();

        IReadOnlyList<IBattleAction> PlayerActions(Combatant player);
    }

    /// <summary>Game-specific battle menu. Polled each frame by the director.</summary>
    public interface IBattleUi
    {
        void Open(BattleEngine battle);
        void Close();
        void ShowResult(ActionResult result);

        /// <summary>Returns true once the player has picked an action and a target.</summary>
        bool TryGetPlayerChoice(Combatant actor, out IBattleAction action, out Combatant target);
    }
}
