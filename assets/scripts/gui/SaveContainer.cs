using Galatime.UI;
using Godot;
using System;

namespace Galatime
{
    public partial class SaveContainer : HBoxContainer
    {
        public Label NameLabel;
        public Label DescriptionLabel;
        public LabelButton DeleteButton;
        public LabelButton PlayButton;

        public int id = 1;

        public override void _Ready()
        {
            NameLabel = GetNode<Label>("VBoxContainer/Name");
            DescriptionLabel = GetNode<Label>("VBoxContainer/Description");
            DeleteButton = GetNode<LabelButton>("DeleteButton");
            PlayButton = GetNode<LabelButton>("PlayButton");

            DeleteButton.PivotOffset = new Vector2(21, 5);
            PlayButton.PivotOffset = new Vector2(14, 5);
        }

        public LabelButton GetDeleteButtonInstance() => DeleteButton;
        public LabelButton GetPlayButtonInstance() => PlayButton;

        public void LoadData(Godot.Collections.Dictionary data)
        {
            if (data != null && data.Count >= 0)
            {
                GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=cyan]Load data[/color]");
                var id = data.ContainsKey("id") ? (int)data["id"] : 0;
                NameLabel.Text = "Save " + (id == 0 ? "?" : id);
                this.id = id;
                DescriptionLabel.Text =
                    $"Chapter " + (data.ContainsKey("chapter") ? (int)data["chapter"] : "?")
                    + " - Day " + (data.ContainsKey("day") ? (int)data["day"] : "?")
                    + " - " + (data.ContainsKey("playtime") ? Math.Round((float)data["playtime"] / 3600, 1) + " h" : "?");
            }
            else
            {
                GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=red]Data is null[/color]");
            }
        }
    }
}
