using Galatime.Settings;
using Godot;
using System.Reflection;
using System;
using System.Collections.Generic;

public partial class SettingsContainer : Control
{
    VBoxContainer SettingsListContainer;

    public override void _Ready()
    {
        SettingsListContainer = GetNode<VBoxContainer>("SettingsListContainer");

        Build();
    }

    public void Build()
    {
        var settings = GetNode<SettingsGlobals>("/root/SettingsGlobals").LoadSettings();

        // Call Change to update the values.
        Change(settings);

        // Get the type of the Settings class.
        Type settingsType = typeof(Settings);

        // Iterate over the fields of the Settings class.
        foreach (FieldInfo field in settingsType.GetFields())
        {
            // Get the name, type, and value of the field.
            string name = field.Name;
            Type type = field.FieldType;
            object value = field.GetValue(settings);

            var convertedName = "";
            var settingNameAttribute = (SettingNameAttribute)Attribute.GetCustomAttribute(field, typeof(SettingNameAttribute));
            convertedName = settingNameAttribute != null ? settingNameAttribute.Name : name;

            // Create a label node to display the name of the field.
            var label = new Label { Text = convertedName, Name = name };

            // Create a VBoxContainer node to hold the UI setting nodes.
            var container = new HBoxContainer();
            container.AddChild(label);

            // Check the type of the field and create a corresponding UI node.
            if (type == typeof(double))
            {
                var slider = new HSlider
                {
                    MinValue = 0,
                    MaxValue = 1,
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    SizeFlagsVertical = SizeFlags.ShrinkCenter,
                    Step = 0.01
                };
                slider.Value = (double)value;
                slider.ValueChanged += (value) => ValueChanged(value, field);
                container.AddChild(slider);
            }
            else if (type == typeof(bool))
            {
                var checkBox = new CheckButton() { ButtonPressed = (bool)value };
                checkBox.Toggled += (value) => ValueChanged(value, field);
                container.AddChild(checkBox);
                GD.Print($"CheckBox value: {checkBox.ButtonPressed}");
            }

            // Add the container node to the scene tree as a child of this node.
            SettingsListContainer.AddChild(container);
        }
    }

    // This function is called when any UI value is changed
    private void ValueChanged<T>(T value, FieldInfo field) { 
        field.SetValue(SettingsGlobals.Settings, value);
        Change(SettingsGlobals.Settings);
    }
    
    /// <summary> Converts a bit value (0-1) to a dB value. </summary>
    static double BitToDb(double value) => value * 80 - 80;

    public void Change(Settings settings) {

        AudioServer.SetBusVolumeDb(0, (float)BitToDb(settings.MasterVolume));
        AudioServer.SetBusVolumeDb(1, (float)BitToDb(settings.MusicVolume));
    }
}
