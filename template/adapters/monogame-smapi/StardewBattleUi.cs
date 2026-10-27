using System.Collections.Generic;
using System.Linq;
using AnyRpg.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AnyRpg.Smapi
{
    /// <summary>Text-box battle menu drawn on the HUD. Up/Down picks, Confirm accepts, Cancel goes back.</summary>
    internal sealed class StardewBattleUi : IBattleUi
    {
        private readonly IGameAdapter _game;
        private BattleEngine _battle;
        private IReadOnlyList<IBattleAction> _actions;
        private int _cursor;
        private IBattleAction _picked;
        private Combatant _chosenTarget;
        private string _lastMessage = "";

        public StardewBattleUi(IGameAdapter game) { _game = game; }

        public void Open(BattleEngine battle)
        {
            _battle = battle;
            _actions = _game.PlayerActions(battle.Players.First());
            _cursor = 0;
            _picked = null;
            _chosenTarget = null;
            _lastMessage = "¡Encuentro!";
        }

        public void Close() => _battle = null;

        public void ShowResult(ActionResult result) => _lastMessage = result.Message;

        public bool TryGetPlayerChoice(Combatant actor, out IBattleAction action, out Combatant target)
        {
            action = null;
            target = null;
            if (_picked == null || (_picked.Targets == TargetKind.SingleEnemy && _chosenTarget == null)) return false;

            action = _picked;
            target = _chosenTarget;
            _picked = null;
            _chosenTarget = null;
            _cursor = 0;
            return true;
        }

        public void OnButton(SButton button, IInputHelper input)
        {
            if (_battle == null || _battle.Current.Team != Team.Player) return;

            int count = _picked == null ? _actions?.Count ?? 0 : LiveEnemies().Count;
            if (button == SButton.Up || button == SButton.W || button == SButton.DPadUp) _cursor = (_cursor + count - 1) % count;
            else if (button == SButton.Down || button == SButton.S || button == SButton.DPadDown) _cursor = (_cursor + 1) % count;
            else if (button.IsActionButton() || button == SButton.Enter || button == SButton.ControllerA)
            {
                if (_picked == null)
                {
                    var a = _actions[_cursor];
                    if (a.MpCost > _battle.Current.Stats.Mp) { _lastMessage = "No alcanza la energía"; return; }
                    _picked = a;
                    _cursor = 0;
                }
                else
                {
                    _chosenTarget = LiveEnemies()[_cursor];
                }
            }
            else if (button == SButton.Escape || button == SButton.ControllerB)
            {
                _picked = null;
                _cursor = 0;
            }
            else return;

            input.Suppress(button);
        }

        public void Draw(SpriteBatch b)
        {
            if (_battle == null) return;
            var font = Game1.smallFont;
            int x = 32, y = Game1.uiViewport.Height - 320, w = 520, h = 288;
            IClickableMenu.drawTextureBox(b, x, y, w, h, Color.White);

            int ty = y + 24;
            foreach (var c in _battle.All)
            {
                var color = !c.IsAlive ? Color.Gray : c == _battle.Current ? Color.DarkGreen : Game1.textColor;
                b.DrawString(font, $"{c.Name}  {c.Stats.Hp}/{c.Stats.MaxHp}", new Vector2(x + 24, ty), color);
                ty += 32;
            }

            var options = _picked == null
                ? (_actions ?? new IBattleAction[0]).Select(a => a.MpCost > 0 ? $"{a.Name} ({a.MpCost})" : a.Name).ToList()
                : LiveEnemies().Select(e => e.Name).ToList();
            int oy = y + 24;
            for (int i = 0; i < options.Count; i++)
            {
                string prefix = _battle.Current.Team == Team.Player && i == _cursor ? "> " : "  ";
                b.DrawString(font, prefix + options[i], new Vector2(x + 300, oy), Game1.textColor);
                oy += 32;
            }

            b.DrawString(font, _lastMessage, new Vector2(x + 24, y + h - 48), Game1.textColor);
        }

        private List<Combatant> LiveEnemies() => _battle.Enemies.Where(e => e.IsAlive).ToList();
    }
}
