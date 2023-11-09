using Galatime.Settings;
using Galatime.Global;
using Godot;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace Galatime.UI;

public partial class SettingsContainer : Control
{
    public VBoxContainer SettingsListContainer;
    private SettingsGlobals SettingsGlobals;

    public override void _Ready()
    {
        SettingsListContainer = GetNode<VBoxContainer>("SettingsListContainer");
        SettingsGlobals = GetNode<SettingsGlobals>("/root/SettingsGlobals");

        Build();
    }

    /// <summary> Builds the UI for the settings by iterating over the fields of the Settings class and creating corresponding UI nodes. </summary>
    public void Build()
    {
        var settings = GetNode<SettingsGlobals>("/root/SettingsGlobals").LoadSettings();

        // Call the UpdateSettings method to update the settings.
        SettingsGlobals.UpdateSettings();

        Type settingsType = typeof(SettingsData);
        foreach (FieldInfo field in settingsType.GetFields())
        {
            // Get the name, type, and value of the field.
            string name = field.Name;
            Type type = field.FieldType;
            object value = field.GetValue(settings);

            var convertedName = "";
            var settingNameAttribute = (SettingPropertiesAttribute)Attribute.GetCustomAttribute(field, typeof(SettingPropertiesAttribute));
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
    private void ValueChanged<T>(T value, FieldInfo field)
    {
        field.SetValue(SettingsGlobals.Settings, value);
        SettingsGlobals.UpdateSettings();
    }
}
