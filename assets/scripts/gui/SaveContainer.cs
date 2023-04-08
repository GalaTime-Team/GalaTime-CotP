using Godot;
using System;

namespace Galatime 
{
    public partial class SaveContainer : HBoxContainer
    {
        public Label nameLabel;
        public Label descriptionLabel;
        public Label deleteButton;
        public Label playButton;

        public override void _Ready()
        {
            nameLabel = GetNode<Label>("VBoxContainer/Name");
            descriptionLabel = GetNode<Label>("VBoxContainer/Description");
            deleteButton = GetNode<Label>("DeleteButton");
            playButton = GetNode<Label>("PlayButton");

            deleteButton.PivotOffset = new Vector2(21, 5);
            playButton.PivotOffset = new Vector2(14, 5);
        }

        public Label getDeleteButtonInstance()
        {
            return deleteButton;
        }

        public Label getPlayButtonInstance()
        {
            return playButton;
        }

        public void loadData(Godot.Collections.Dictionary data)
        {
            if (data != null && data.Count >= 0)
            {
                GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=cyan]Load data[/color]");
                nameLabel.Text = "Save " + (data.ContainsKey("id") ? (int)data["id"] : "?");
                descriptionLabel.Text =
                    $"Chapter " + (data.ContainsKey("chapter") ? (int)data["chapter"] : "?")
                    + " - Day " + (data.ContainsKey("day") ? (int)data["day"] : "?")
                    + " - " + (data.ContainsKey("playtime") ? Math.Round((float)data["playtime"] / 3600, 1) + " h" : "?");
            }
            else
            {
                GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=red]Data is null[/color]");
            }
        }

        public override void _Process(double delta)
        {

        }
    }
}
