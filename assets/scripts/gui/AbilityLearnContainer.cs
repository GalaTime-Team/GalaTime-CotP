using Godot;
using System;

namespace Galatime
{
    public partial class AbilityLearnContainer : VBoxContainer
    {
        private Label learnButton;
        private Label nameLabel;
        private RichTextLabel learnButtonLabel;

        private Color learnButtonDefaultColor = new Color(0.196078f, 0.803922f, 0.196078f);
        private Color learnButtonDisabledColor = new Color(0.09411764889956f, 0.39215686917305f, 0.09411764889956f);

        private bool learnButtonDisabled = false;

        private PlayerVariables _playerVariables;

        public AbilityData abilityData = new();
        private bool _visible;
        public bool visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                if (value)
                {
                    updateData(-1);
                }
                Visible = value;
            }
        }

        public override void _Ready()
        {
            learnButton = GetNode<Label>("LearnButtonContainer/LearnButton");
            learnButtonLabel = GetNode<RichTextLabel>("LearnButtonContainer/Label");
            nameLabel = GetNode<Label>("Label");

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            learnButton.MouseEntered += () => _onUpgradeButtonMouseEntered();
            learnButton.MouseExited += () => _onUpgradeButtonMouseExited();
            learnButton.GuiInput += (InputEvent @event) => _onLearnButtonGuiInput(@event);

            PlayerVariables.onXpChanged += updateData;
        }

        public override void _ExitTree()
        {
            PlayerVariables.onXpChanged -= updateData;
        }

        public string CostXPString => $"{abilityData.CostXP}/{_playerVariables.Player.Xp} [color=32cd32]XP[/color]";

        /// <summary>
        /// Updates the data about the stat.
        /// </summary>
        /// <param name="xp">The value to update the stat with. If set to -1, data is taken from an external source.</param>
        public void updateData(float xp = -1)
        {
            try
            {
                nameLabel.Text = $"Learn ability \"{abilityData.Name}\"?";
                if (_playerVariables.learnAbility(abilityData, true) == LearnedStatus.noRequiredPath)
                {
                    learnButtonLabel.Text = $"{CostXPString} You need to open the previous path";
                    _setButtonDisabled(true);
                    return;
                }
                else if (_playerVariables.learnAbility(abilityData, true) == LearnedStatus.noEnoughCurrency)
                {
                    _setButtonDisabled(true);
                    learnButtonLabel.Text = $"{CostXPString} You don't have enough XP";
                }
                else
                {
                    _setButtonDisabled(false);
                    learnButtonLabel.Text = CostXPString;
                }
            }
            catch (Exception e)
            {
                GD.PrintRich($"[color=purple]LEARN CONTAINER[/color] {e.Message} {e.Source} {e.StackTrace}");
            }
        }

        private void _onUpgradeButtonMouseEntered()
        {
            if (!learnButtonDisabled)
            {
                learnButton.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            }
        }

        private void _onUpgradeButtonMouseExited()
        {
            if (!learnButtonDisabled)
            {
                learnButton.AddThemeColorOverride("font_color", learnButtonDefaultColor);
            }
        }

        private void _setButtonDisabled(bool value)
        {
            if (value)
            {
                learnButton.AddThemeColorOverride("font_color", learnButtonDisabledColor);
                learnButton.MouseDefaultCursorShape = CursorShape.Arrow;
            }
            else
            {
                learnButton.AddThemeColorOverride("font_color", learnButtonDefaultColor);
                learnButton.MouseDefaultCursorShape = CursorShape.PointingHand;
            }
            learnButtonDisabled = value;
        }

        private void _onLearnButtonGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton @mouseEvent)
            {
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    if (!abilityData.IsEmpty)
                    {
                        var result = _playerVariables.learnAbility(abilityData);
                        if (result == LearnedStatus.ok)
                        {
                            learnButton.AddThemeColorOverride("font_color", learnButtonDefaultColor);
                            Visible = false;
                        }
                        else
                        {
                            GD.Print("Can't learn ability, PlayerVariables return false");
                        }
                    }
                    else
                    {
                        GD.Print("Can't learn ability, null");
                    }
                }
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
