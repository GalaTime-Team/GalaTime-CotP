using Godot;

using Galatime.Global;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.UI;

public partial class EnemiesPageContainer : Control
{
    #region Nodes
    public VBoxContainer List;
    public RichTextLabel EntryPanel;
    public TextureRect EntryIcon;
    public LabelButton EntryButton;
    #endregion

    public List<Node> EntryButtons = new();

    public override void _Ready()
    {
        #region Get nodes
        List = GetNode<VBoxContainer>("ListScroll/ListMargin/List");
        EntryPanel = GetNode<RichTextLabel>("EntryPanel");
        EntryIcon = GetNode<TextureRect>("EntryIcon");
        EntryButton = GetNode<LabelButton>("LabelButton");
        #endregion

        UpdateEntireList();
        PlayerVariables.Instance.OnDiscoveredEnemiesChanged += UpdateEntireList;

        // Select first entry by default if discovered
        var first = EnemiesList.EnemiesData.First();
        if (CheckDiscovered(first.Value.NumID))
            SelectEntry(first.Key);
    }

    public override void _ExitTree()
    {
        PlayerVariables.Instance.OnDiscoveredEnemiesChanged -= UpdateEntireList;
    }

    public void UpdateEntireList()
    {
        // Clear list firstly
        EntryButtons.ForEach(x => x.QueueFree());
        EntryButtons.Clear();

        foreach (var item in EnemiesList.EnemiesData)
        {
            // Added button to the list for each entry
            var button = EntryButton.Duplicate() as LabelButton;

            // If enemy is not discovered, disable button and don't reveal name
            if (!CheckDiscovered(item.Value.NumID))
            {
                button.ButtonText = $"{item.Value.GetEnemyNumID()} - ?";
                button.Disabled = true;
            }
            else
            {
                button.ButtonText = $"{item.Value.GetEnemyNumID()} - {item.Value.Name}";
                button.Pressed += () => SelectEntry(item.Key);
            }

            // Added button to the list for each entry
            List.AddChild(button);
            EntryButtons.Add(button);
            button.Visible = true;
        }
    }

    public static bool CheckDiscovered(int nId) => PlayerVariables.Instance.DiscoveredEnemies.Any(x => x == nId);

    public void SelectEntry(string index)
    {
        var enemy = EnemiesList.EnemiesData[index];

        EntryPanel.Clear();
        EntryPanel.AppendText(enemy.Entry);

        var icon = GD.Load<Texture2D>(enemy.IconPath);
        if (icon != null) EntryIcon.Texture = icon;
    }
}
