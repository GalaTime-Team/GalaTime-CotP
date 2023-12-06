using System.Linq;
using System.Collections.Generic;
using Godot;
using System;

namespace Galatime
{
    public partial class AbilitiesContainer : ScrollContainer
    {
        private Godot.Collections.Array<Node> AbilityItemsContainers;
        private Panel AbilitiesPanel;
        private AbilitiesChoiseContainer AbilityChoiseContainer;
        private AbilityLearnContainer AbilityLearnContainer;

        private AbilityContainerItem CurrentAbilityItemContainer;

        private List<AbilityContainerItem> AbilityContainerItems = new();

        private const float MoveDuration = 0.3f;

        private PlayerVariables PlayerVariables;


        public override void _Ready()
        {
            AbilitiesPanel = GetNode<Panel>("Panel");
            AbilityItemsContainers = GetTree().GetNodesInGroup("abilityItem");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            AbilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("Panel/AbilitiesChoiseContainer");
            AbilityLearnContainer = GetNode<AbilityLearnContainer>("Panel/AbilityLearnContainer");

            var tempChildren = GetNode("Panel").GetChildren();
            foreach (var child in tempChildren) if (child is AbilityContainerItem tc) AbilityContainerItems.Add(tc);
            InstantiateAbilities();

            PlayerVariables.OnAbilityLearned += OnLearnedAbilitiesUpdated;
            AbilityLearnContainer.OnLearned += OnAbilityLearned;
        }

        public override void _ExitTree() {
            PlayerVariables.OnAbilityLearned -= OnLearnedAbilitiesUpdated;
            AbilityLearnContainer.OnLearned -= OnAbilityLearned;
        }

        private void OnAbilityLearned() => CurrentAbilityItemContainer?.SetLearned(true, true);

        private void OnLearnedAbilitiesUpdated() {
            foreach (var i in AbilityContainerItems) 
            {
                var learned = PlayerVariables.AbilityIsLearned(i.AbilityData.ID);
                i.SetLearned(learned);
            }
        }

        private void InstantiateAbilities()
        {
            foreach (var item in AbilityContainerItems)
            {
                item.GuiInput += (InputEvent @event) => GuiItemInput(@event, item);

                var required = item.AbilityData.RequiredIDs;

                for (int i = 0; i < required.Length; i++)
                {
                    var targetItem = FindItemByAbilityId(required[i]);
                    if (targetItem is null) continue;
                    var points = new Vector2[2];

                    var position0 = targetItem.Position;
                    position0.X += 8;
                    position0.Y += 8;

                    var position1 = item.Position;
                    position1.X += 8;
                    position1.Y += 8;

                    points[0] = position0;
                    points[1] = position1;

                    var line = GetLinkLine();
                    line.Points = points;
                    GetNode("Panel").AddChild(line);
                }
            }
        }

        /// <summary> Returns a Line2D node with style. </summary>
        public Line2D GetLinkLine()
        {
            var line = new Line2D
            {
                Texture = GD.Load<Texture2D>("res://sprites/test/chain.png"),
                TextureMode = Line2D.LineTextureMode.Stretch,
                Gradient = new Gradient(),
                Width = 8,
                ZIndex = 0
            };

            return line;
        }

        public AbilityContainerItem FindItemByAbilityId(string id) => AbilityContainerItems.FirstOrDefault(item => item.AbilityName == id);

        public void GuiItemInput(InputEvent @event, AbilityContainerItem item)
        {
            if (@event is InputEventMouseButton @mouseEvent)
            {
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    item.AnimationPlayer.Advance(999);
                    
                    if (!PlayerVariables.AbilityIsLearned(item.AbilityName))
                    {
                        CurrentAbilityItemContainer = item;
                        
                        AbilityLearnContainer.abilityData = item.AbilityData;
                        MoveChoise(AbilityLearnContainer, item, !AbilityLearnContainer.visible);
                        AbilityChoiseContainer.Visible = false;
                        FadeColor(AbilityChoiseContainer, true);
                        AbilityLearnContainer.visible = true;
                    }
                    else
                    {
                        CurrentAbilityItemContainer = null;

                        AbilityChoiseContainer.ChoiceId = item.AbilityName;
                        MoveChoise(AbilityChoiseContainer, item, !AbilityChoiseContainer.Visible);
                        AbilityLearnContainer.visible = false;
                        FadeColor(AbilityLearnContainer, true);
                        AbilityChoiseContainer.Visible = true;
                    }
                }
            }
        }

        void MoveChoise(Control container, Control item, bool instant)
        {
            var tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Sine);
            var initialPosition = container.GlobalPosition;

            // Getting position of item.
            var position = item.GlobalPosition;

            // Adjusting position relative to container. This means this will be placed in middle of container and so on.
            position.X -= container.Size.X - item.Size.X;
            position.Y -= container.Size.Y * 2f + 8f;

            // This needed for non-sequential appearance.
            if (!instant) tween.TweenMethod(Callable.From<Vector2>(x => container.GlobalPosition = x), initialPosition, position, MoveDuration).SetDelay(0.05);
            else {
                container.GlobalPosition = position;
                FadeColor(container, false);
            }
        }

        private void FadeColor(Control container, bool bit) {
            var tween = GetTree().CreateTween();
            var tc = new Color(1, 1, 1, 0);
            var oc = new Color(1, 1, 1, 1);
            tween.TweenMethod(Callable.From<Color>(x => container.Modulate = x), bit ? oc : tc, bit ? tc : oc, MoveDuration);
        }
    }
}
