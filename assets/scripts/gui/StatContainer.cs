using Godot;
using System;
using Galatime;
using static Galatime.GalatimeConstants;

namespace Galatime
{
    public struct IconsPaths
    {
        public static string physicalAttack = "2";
        public static string magicalAttack = "res://sprites/gui/stats/magicall-attack.png";
        public static string physicalDefence = "res://sprites/gui/stats/physical-defence-icon.png";
        public static string magicalDefence = "res://sprites/gui/stats/magical-defence-icon.png";
        public static string health = "res://sprites/gui/stats/health-icon.png";
        public static string mana = "res://sprites/gui/stats/mana-icon.png";
        public static string agility = "res://sprites/gui/stats/agility-icon.png";
        public static string stamina = "res://sprites/gui/stats/stamina-icon.png";
        public static string manaRegen = "res://sprites/gui/stats/mana-icon.png";
        public static string staminaRegen = "res://sprites/gui/stats/stamina-regen-icon.png";
    }

    public partial class StatContainer : VBoxContainer
    {
        public enum Status
        {
            noEnough,
            ok
        }

        [Signal] public delegate void on_upgradeEventHandler(int id);

        private PlayerGui playerGui;

        private Label upgradeButton;
        private Label nameLabel;
        private TextureProgressBar amountProgressBar;
        private Label amountLabel;
        private TextureRect iconTextureRect;
        private RichTextLabel neededCurencyLabel;
        private AnimationPlayer animationPlayer;

        private Tooltip tooltip;

        EntityStat stat;

        private Color upgradeButtonDefaultColor = new Color(0.196078f, 0.803922f, 0.196078f);

        public override void _Ready()
        {
            amountProgressBar = GetNode<TextureProgressBar>("Container/AmountProgressBar");
            amountLabel = GetNode<Label>("Container/AmountLabel");
            neededCurencyLabel = GetNode<RichTextLabel>("UpgradeContainer/NeededCurencyLabel");
            iconTextureRect = GetNode<TextureRect>("Container/TextureRect");
            nameLabel = GetNode<Label>("Label");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            tooltip = GetNode<Tooltip>("../../../Tooltip");

            upgradeButton = GetNode<Label>("UpgradeContainer/UpgradeButton");
            upgradeButton.Connect("mouse_entered", new Callable(this, "_onUpgradeButtonMouseEntered"));
            upgradeButton.Connect("mouse_exited", new Callable(this, "_onUpgradeButtonMouseExited"));
            upgradeButton.Connect("gui_input", new Callable(this, "_onUpgradeButtonGuiInput"));
        }

        public void playAnimation(Status status)
        {
            animationPlayer.Stop();
            switch (status)
            {
                case Status.noEnough:
                    animationPlayer.Play("no");
                    break;
                case Status.ok:
                    animationPlayer.Play("levelup");
                    break;
                default:
                    break;
            }
        }

        private void _onUpgradeButtonMouseEntered()
        {
            upgradeButton.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            tooltip._display(stat);
        }

        private void _onUpgradeButtonMouseExited()
        {
            upgradeButton.AddThemeColorOverride("font_color", upgradeButtonDefaultColor);
            tooltip._hide();    
        }

        private void _onUpgradeButtonGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    EmitSignal(SignalName.on_upgrade, (int)stat.type);
                }
            }
        }

        public void loadData(EntityStat stat, int xpAmount)
        {
            var texture = GD.Load<Texture2D>(stat.iconPath);
            iconTextureRect.Texture = texture != null ? texture : null;
            amountProgressBar.Value = stat.value;
            amountLabel.Text = $"{stat.value}/150";
            nameLabel.Text = stat.name;
            neededCurencyLabel.Text = $"100/{xpAmount} [color=32cd32]XP[/color]";

            this.stat = stat;
        }
    }
}