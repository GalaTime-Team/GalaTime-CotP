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

        /// <summary>
        /// Loads save data from a SaveData object (new format).
        /// </summary>
        public void LoadData(SaveData data)
        {
            GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=cyan]Load data (SaveData)[/color]");
            NameLabel.Text = $"Save {id}";
            
            if (data == null || data.IsEmpty)
            {
                DescriptionLabel.Text = "No saved data";
                DeleteButton.Disabled = true;
                return;
            }
            
            var playtimeHours = Math.Round(data.Playtime / 3600f, 1);
            DescriptionLabel.Text = $"Chapter {data.Chapter} - Day {data.Day} - {playtimeHours} h";
            DeleteButton.Disabled = false;
        }

        /// <summary>
        /// Loads save data from a Godot Dictionary (legacy format, for backwards compatibility).
        /// </summary>
        public void LoadData(Godot.Collections.Dictionary data)
        {
            GD.PrintRich("[color=green]SAVE CONTAINER[/color]: [color=cyan]Load data (Dictionary)[/color]");
            NameLabel.Text = $"Save {id}";
            
            if (data == null || data.Count == 0)
            {
                DescriptionLabel.Text = "No saved data";
                DeleteButton.Disabled = true;
                return;
            }
            
            // Try to convert to SaveData for consistent handling
            var saveData = SaveData.FromDictionary(data);
            LoadData(saveData);
        }
    }   
}
