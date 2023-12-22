using ExtensionMethods;
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
            GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=cyan]Load data[/color]");
            NameLabel.Text = $"Save {id}";
            var chapter = (string)data.GetOrDefaultValue("chapter", "?");
            var day = (string)data.GetOrDefaultValue("day", "?");
            var playtime = (string)data.GetOrDefaultValue(Math.Round((float)data.GetOrDefaultValue("playtime", 0) / 3600, 1).ToString(), "?");
            if (chapter == "?" && day == "?") 
            {
                DescriptionLabel.Text = "No saved data";
                DeleteButton.Disabled = true;
                return;
            }
            DescriptionLabel.Text = $"Chapter {chapter} - Day {day} - {playtime} h";
        }
    }   
}
