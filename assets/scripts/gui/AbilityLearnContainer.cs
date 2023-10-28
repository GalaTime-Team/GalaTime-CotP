using Godot;
using System;

namespace Galatime
{
    public partial class AbilityLearnContainer : VBoxContainer
    {
        private Label learnButton;
        private Label NameLabel;
        private RichTextLabel LearnButtonLabel;

        /// <summary> Default color for the button. This color is used when the button is in its enabled state. </summary>
        private Color DefaultColorButton = new(0.196078f, 0.803922f, 0.196078f);
        /// <summary> Disabled color for the button. This color is used when the button is disabled and cannot be interacted with. </summary>
        private Color DisabledColorButton = new(0.09411764889956f, 0.39215686917305f, 0.09411764889956f);

        /// <summary> Called when the learning is successful when the button is clicked. </summary>
        public Action OnLearned;

        private bool LearnButtonDisabled = false;

        private PlayerVariables PlayerVariables;

        public AbilityData abilityData = new();
        private bool _visible;
        public bool visible
        {
            get => _visible;
            set
            {
                _visible = value;
                if (value) UpdateData(-1);
                Visible = value;
            }
        }

        public override void _Ready()
        {
            learnButton = GetNode<Label>("LearnButtonContainer/LearnButton");
            LearnButtonLabel = GetNode<RichTextLabel>("LearnButtonContainer/Label");
            NameLabel = GetNode<Label>("Label");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            learnButton.MouseEntered += () => OnUpgradeButtonMouseEntered();
            learnButton.MouseExited += () => OnUpgradeButtonMouseExited();
            learnButton.GuiInput += (InputEvent @event) => OnLearnButtonGuiInput(@event);

            PlayerVariables.OnXpChanged += UpdateData;
        }

        public override void _ExitTree()
        {
            PlayerVariables.OnXpChanged -= UpdateData;
        }

        public string CostXPString => $"{abilityData.CostXP}/{PlayerVariables.Player.Xp} [color=32cd32]XP[/color]";

        /// <summary>
        /// Updates the data about the stat.
        /// </summary>
        /// <param name="xp">The value to update the stat with. If set to -1, data is taken from an external source.</param>
        public void UpdateData(float xp = -1)
        {
            try
            {
                NameLabel.Text = $"Learn ability \"{abilityData.Name}\"?";
                if (PlayerVariables.LearnAbility(abilityData, true) == LearnedStatus.noRequiredPath)
                {
                    LearnButtonLabel.Text = $"{CostXPString} You need to open the previous path";
                    SetButtonDisabled(true);
                    return;
                }
                else if (PlayerVariables.LearnAbility(abilityData, true) == LearnedStatus.noEnoughCurrency)
                {
                    SetButtonDisabled(true);
                    LearnButtonLabel.Text = $"{CostXPString} You don't have enough XP";
                }
                else
                {
                    SetButtonDisabled(false);
                    LearnButtonLabel.Text = CostXPString;
                }
            }
            catch (Exception e)
            {
                GD.PrintRich($"[color=purple]LEARN CONTAINER[/color] {e.Message} {e.Source} {e.StackTrace}");
            }
        }

        private void OnUpgradeButtonMouseEntered()
        {
            if (!LearnButtonDisabled)
            {
                learnButton.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            }
        }

        private void OnUpgradeButtonMouseExited()
        {
            if (!LearnButtonDisabled)
            {
                learnButton.AddThemeColorOverride("font_color", DefaultColorButton);
            }
        }

        private void SetButtonDisabled(bool value)
        {
            if (value)
            {
                learnButton.AddThemeColorOverride("font_color", DisabledColorButton);
                learnButton.MouseDefaultCursorShape = CursorShape.Arrow;
            }
            else
            {
                learnButton.AddThemeColorOverride("font_color", DefaultColorButton);
                learnButton.MouseDefaultCursorShape = CursorShape.PointingHand;
            }
            LearnButtonDisabled = value;
        }

        private void OnLearnButtonGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton @mouseEvent)
            {
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    if (!abilityData.IsEmpty)
                    {
                        var result = PlayerVariables.LearnAbility(abilityData);
                        if (result == LearnedStatus.ok)
                        {
                            learnButton.AddThemeColorOverride("font_color", DefaultColorButton);
                            Visible = false;
                            OnLearned?.Invoke();
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
