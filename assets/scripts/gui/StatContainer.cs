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
            maximum,
            ok
        }

        public enum ContainerState
        {
            normal,
            maximum
        }

        private ContainerState state = ContainerState.normal;
        public ContainerState State
        {
            get
            {
                return state;
            }
            set
            {
                switch (value)
                {
                    case ContainerState.normal:
                        break;
                    case ContainerState.maximum:
                        animationPlayer.Stop();
                        tooltip._hide();

                        upgradeButton.Text = "MAXIMUM";
                        upgradeButton.MouseDefaultCursorShape = CursorShape.Arrow;
                        upgradeButton.AddThemeColorOverride("font_color", new Color(1, 0.86274510622025f, 0.19607843458652f));
                        amountProgressBar.TintProgress = upgradeButtonDefaultColor;
                        break;
                }
                state = value;
            }
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

            upgradeButton.MouseEntered += _onUpgradeButtonMouseEntered;
            upgradeButton.MouseExited += _onUpgradeButtonMouseExited;
            upgradeButton.GuiInput += _onUpgradeButtonGuiInput;
        }

        public void playAnimation(Status status)
        {
            if (stat.Value >= 150)
            {
                State = ContainerState.maximum;
                return;
            }
            animationPlayer.Stop();
            switch (status)
            {
                case Status.noEnough:
                    animationPlayer.Play("no");
                    upgradeButton.Text = "NO ENERGY";
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
            if (State != ContainerState.normal) return;
            upgradeButton.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            tooltip._display(stat);
        }

        private void _onUpgradeButtonMouseExited()
        {
            if (State != ContainerState.normal) return;
            upgradeButton.AddThemeColorOverride("font_color", upgradeButtonDefaultColor);
            tooltip._hide();    
        }

        private void _onUpgradeButtonGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && State == ContainerState.normal)
                {
                    EmitSignal(SignalName.on_upgrade, (int)stat.type);
                }
            }
        }

        public void loadData(EntityStat stat, int xpAmount)
        {
            if (stat.Value >= 150)
            {
                State = ContainerState.maximum;
            }
            var texture = GD.Load<Texture2D>(stat.iconPath);
            iconTextureRect.Texture = texture != null ? texture : null;
            amountProgressBar.Value = stat.Value;
            amountLabel.Text = $"{stat.Value}/150";
            nameLabel.Text = stat.name;
            neededCurencyLabel.Text = $"100/{xpAmount} [color=32cd32]XP[/color]";

            this.stat = stat;
        }
    }
}