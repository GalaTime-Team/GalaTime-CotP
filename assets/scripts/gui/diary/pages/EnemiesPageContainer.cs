using Godot;

using Galatime.Global;
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

    public override void _Ready()
    {
        #region Get nodes
        List = GetNode<VBoxContainer>("ListScroll/ListMargin/List");
        EntryPanel = GetNode<RichTextLabel>("EntryPanel");
        EntryIcon = GetNode<TextureRect>("EntryIcon");
        EntryButton = GetNode<LabelButton>("LabelButton");
        #endregion

        foreach (var item in EnemiesList.EnemiesData)
        {
            // Added button to the list for each entry
            var button = EntryButton.Duplicate() as LabelButton;
            button.ButtonText = $"{item.Value.GetEnemyNumID()} - {item.Value.Name}";
            button.Pressed += () => SelectEntry(item.Key);
            List.AddChild(button);
            button.Visible = true;
        }

        // Select first entry by default
        SelectEntry(EnemiesList.EnemiesData.First().Key);
    }

    public void SelectEntry(string index)
    {
        var enemy = EnemiesList.EnemiesData[index];

        EntryPanel.Clear();
        EntryPanel.AppendText(enemy.Entry);

        var icon = GD.Load<Texture2D>(enemy.IconPath);
        if (icon != null) EntryIcon.Texture = icon;
    }
}
