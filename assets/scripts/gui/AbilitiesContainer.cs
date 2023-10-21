using System.Linq;
using Godot;

namespace Galatime
{
    public partial class AbilitiesContainer : ScrollContainer
    {
        private Godot.Collections.Array<Node> AbilityItemsContainers;
        private Panel AbilitiesPanel;
        private AbilitiesChoiseContainer AbilityChoiseContainer;
        private AbilityLearnContainer AbilityLearnContainer;
        private Godot.Collections.Array<AbilityContainerItem> AbilityContainerItems = new();

        private PlayerVariables PlayerVariables;

        private Tooltip Tooltip;

        public override void _Ready()
        {
            Tooltip = GetNode<Tooltip>("../Tooltip");
            AbilitiesPanel = GetNode<Panel>("Panel");
            AbilityItemsContainers = GetTree().GetNodesInGroup("abilityItem");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            AbilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("Panel/AbilitiesChoiseContainer");
            AbilityLearnContainer = GetNode<AbilityLearnContainer>("Panel/AbilityLearnContainer");

            var tempChildren = GetNode("Panel").GetChildren();
            foreach (var child in tempChildren) if (child is AbilityContainerItem tc) AbilityContainerItems.Add(tc);
            InstantiateAbilities();
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

        public void _onAbilityLearned()
        {
            foreach (var i in AbilityContainerItems)
            {
                if (PlayerVariables.AbilityIsLearned(i.Name))
                {
                    // i.setLearned();
                }
            }
        }

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
                    AbilityLearnContainer.visible = false;
                    AbilityChoiseContainer.Visible = false;
                    if (!PlayerVariables.AbilityIsLearned(item.AbilityName))
                    {
                        var position = item.GlobalPosition;
                        position.X -= 288;
                        position.Y -= 70;
                        AbilityLearnContainer.abilityData = item.AbilityData;
                        AbilityLearnContainer.visible = true;
                        AbilityLearnContainer.GlobalPosition = position;
                    }
                    else
                    {
                        var position = item.GlobalPosition;
                        position.X -= 80;
                        position.Y -= 70;
                        AbilityChoiseContainer.Visible = true;
                        AbilityChoiseContainer.GlobalPosition = position;
                        AbilityChoiseContainer.ChoiceId = item.AbilityName;
                    }
                }
            }
        }
    }
}
