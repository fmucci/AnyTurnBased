using System;

namespace AnyTurnBased.Core
{
    public enum ReactionKind { None, Dodge, Parry }

    /// <summary>
    /// One enemy attack against a player with reactive.dodge-parry on. The impact lands at ImpactAt seconds;
    /// one press is allowed, and it only counts inside its window. Pressing early or late is a miss.
    /// </summary>
    public sealed class ReactionTimer
    {
        public float Elapsed { get; private set; }
        public float ImpactAt { get; }
        public float DodgeWindow { get; }
        public float ParryWindow { get; }

        /// <summary>True once the player pressed or the dodge window closed.</summary>
        public bool Resolved { get; private set; }
        public ReactionKind Outcome { get; private set; }

        /// <summary>Resolved and the impact moment has been reached, so the hit can be shown.</summary>
        public bool Done => Resolved && Elapsed >= ImpactAt;

        public ReactionTimer(float impactAt, ReactionRules rules)
        {
            if (impactAt <= 0) throw new ArgumentOutOfRangeException(nameof(impactAt), "impact must be in the future");
            ImpactAt = impactAt;
            DodgeWindow = rules.DodgeWindow;
            ParryWindow = rules.ParryWindow;
        }

        public void Advance(float dt)
        {
            Elapsed += dt;
            if (!Resolved && Elapsed > ImpactAt + DodgeWindow / 2) Resolve(ReactionKind.None);
        }

        public void Press(ReactionKind kind)
        {
            if (Resolved || kind == ReactionKind.None) return;
            float window = kind == ReactionKind.Parry ? ParryWindow : DodgeWindow;
            Resolve(Math.Abs(Elapsed - ImpactAt) <= window / 2 ? kind : ReactionKind.None);
        }

        private void Resolve(ReactionKind outcome)
        {
            Resolved = true;
            Outcome = outcome;
        }
    }

    /// <summary>
    /// Implemented by the battle UI when the game uses reactive.dodge-parry. The UI draws the wind-up from
    /// the timer (the world is frozen, so the game's own attack animation is not visible).
    /// </summary>
    public interface IReactionUi
    {
        void BeginReaction(Combatant attacker, Combatant target, ReactionTimer timer);

        /// <summary>The key pressed this frame, or None.</summary>
        ReactionKind PollReaction();

        void EndReaction(ReactionKind outcome);
    }
}
