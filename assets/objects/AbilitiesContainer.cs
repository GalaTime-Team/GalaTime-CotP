using Galatime;
using Godot;
using System;

public partial class AbilitiesContainer : ScrollContainer
{
    private Godot.Collections.Array<Node> _abilityItemsContainers;
    private Panel _abilitiesPanel;
    private AbilitiesChoiseContainer abilityChoiseContainer;
    private AbilityLearnContainer abilityLearnContainer;
    private Godot.Collections.Array<AbilityContainerItem> abilityContainerItems = new Godot.Collections.Array<AbilityContainerItem>();

    private PlayerVariables _playerVariables;

    private Tooltip _tooltip;

    public override void _Ready()
    {
        _tooltip = GetNode<Tooltip>("../Tooltip");
        _abilitiesPanel = GetNode<Panel>("Panel");
        _abilityItemsContainers = GetTree().GetNodesInGroup("abilityItem");

        _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        _playerVariables.ability_learned += () => _onAbilityLearned();

        abilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("Panel/AbilitiesChoiseContainer");
        abilityLearnContainer = GetNode<AbilityLearnContainer>("Panel/AbilityLearnContainer");
        var tempChildren = GetNode("Panel").GetChildren();

        // Linking the abilities

        // Creating a line

        foreach (var child in tempChildren)
        {
            if (child is AbilityContainerItem tc)
            {
                abilityContainerItems.Add(tc);
            }
        }
        foreach (var item in abilityContainerItems)
        {
            item.abilityData = GalatimeGlobals.getAbilityById(item.abilityName);
            if (item.abilityData.ContainsKey("icon")) item.Texture = GD.Load<Texture2D>((string)item.abilityData["icon"]);

            item.GuiInput += (InputEvent @event) => _guiItemInput(@event, item);
            item.MouseEntered += () => _itemMouseEntered(item.abilityData);
            item.MouseExited += () => _itemMouseExited();
        }
        foreach (var item in abilityContainerItems)
        {
            if (item.abilityData.ContainsKey("required"))
            {
                var required = (Godot.Collections.Array)item.abilityData["required"];
                if (required is null || required.Count <= 0)
                {
                    GD.Print("required is null");
                    continue;
                }
                for (int i = 0; i < required.Count; i++)
                {
                    var targetItem = findItemByAbilityId((string)required[i]);
                    if (targetItem is null)
                    {
                        GD.Print("target item is null");
                        continue;
                    }
                    var points = new Vector2[2];

                    var position0 = targetItem.Position;
                    position0.X += 8;
                    position0.Y += 8;

                    var position1 = item.Position;
                    position1.X += 8;
                    position1.Y += 8;

                    points[0] = position0;
                    points[1] = position1;

                    var line = getLinkLine();
                    line.Points = points;
                    GetNode("Panel").AddChild(line);
                }
            }
            else
            {
                GD.Print("no required");
            }
        }
    }

    private void _onAbilityLearned()
    {
        foreach (var i in abilityContainerItems)
        {
            if (PlayerVariables.abilityIsLearned(i.abilityName))
            {
                i.setLearned();
            }
        }
    }

    public Line2D getLinkLine()
    {
        var line = new Line2D();
        line.Texture = GD.Load<Texture2D>("res://sprites/test/chain.png");
        line.TextureMode = Line2D.LineTextureMode.Stretch;
        line.Gradient = new Gradient();
        line.Width = 8;
        line.ZIndex = 0;

        return line;
    }

    public AbilityContainerItem findItemByAbilityId(string id)
    {
        foreach (var item in abilityContainerItems)
        {
            if (item.abilityName == id)
            {
                return item;
            }
        }
        return null;
    }

    public void _itemMouseEntered(Godot.Collections.Dictionary data)
    {
        _tooltip._display(data);
    }

    public void _itemMouseExited()
    {
        _tooltip._hide();
    }

    public void _guiItemInput(InputEvent @event, AbilityContainerItem item)
    {
        if (@event is InputEventMouseButton)
        {
            var @mouseEvent = @event as InputEventMouseButton;
            if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                abilityLearnContainer.visible = false;
                abilityChoiseContainer.Visible = false;
                if (!PlayerVariables.abilityIsLearned(item.abilityName))
                {
                    var position = item.GlobalPosition;
                    position.X -= 288;
                    position.Y -= 70;
                    abilityLearnContainer.abilityData = item.abilityData;
                    abilityLearnContainer.visible = true;
                    abilityLearnContainer.GlobalPosition = position;
                }
                else
                {
                    if (item.abilitySetCountdown.TimeLeft <= 0)
                    {
                        var position = item.GlobalPosition;
                        position.X -= 80;
                        position.Y -= 70;
                        abilityChoiseContainer.Visible = true;
                        abilityChoiseContainer.GlobalPosition = position;
                        abilityChoiseContainer.choiseId = item.abilityName;
                    }
                }
            }
        }
    }
}

