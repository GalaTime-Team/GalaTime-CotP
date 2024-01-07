using Galatime.Global;
using Galatime.Settings;
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Galatime.UI;

public partial class SettingsContainer : Control
{
    public VBoxContainer SettingsListContainer;
    private SettingsGlobals SettingsGlobals;

    /// <summary> The first control when UI is builded. </summary>
    public Control FirstControl;

    public VBoxContainer CategoriesList;
    public LabelButton CategoryButton;

    public List<Control> CategoryControls = new();
    public List<LabelButton> CategoryButtons = new();

    public SettingsData SettingsData;

    public override void _Ready()
    {
        SettingsListContainer = GetNode<VBoxContainer>("SettingsListContainer");
        SettingsGlobals = GetNode<SettingsGlobals>("/root/SettingsGlobals");

        CategoriesList = GetNode<VBoxContainer>("CategoriesList");
        CategoryButton = GetNode<LabelButton>("CategoriesList/CategoryButton");

        Build();
    }

    /// <summary> Builds the UI for the settings by iterating over the fields of the Settings class and creating corresponding UI nodes. </summary>
    public void Build()
    {
        SettingsData = GetNode<SettingsGlobals>("/root/SettingsGlobals").LoadSettings();

        // Call the UpdateSettings method to update the settings.
        SettingsGlobals.UpdateSettings();

        var settingsType = typeof(SettingsData);
        var fields = settingsType.GetFields();

        // Iterate over the fields of the Settings class and create corresponding UI nodes.
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var settingAttribute = (SettingPropertyAttribute)Attribute.GetCustomAttribute(field, typeof(SettingPropertyAttribute));
            var convertedName = settingAttribute != null ? settingAttribute.Name : field.Name;

            // Build the category in the UI. 
            BuildCategory(convertedName, field.GetValue(SettingsData), field.FieldType);
        }

        FirstControl = CategoryButtons[0];
    }

    public void BuildCategory(string name, object obj, Type type)
    {
        // Adding category to the list and in the UI.
        var listControl = SettingsListContainer.Duplicate() as Control;
        AddChild(listControl);
        CategoryControls.Add(listControl);

        // Create a button to display the name of the category.
        var categoryButton = CategoryButton.Duplicate() as LabelButton;
        categoryButton.Visible = true;

        // Add category button to the categories list in the UI.
        CategoriesList.AddChild(categoryButton);
        CategoryButtons.Add(categoryButton);
        categoryButton.ButtonText = name;

        // When the category button is pressed, show the list of settings for the category.
        categoryButton.Pressed += () => CategoryControls.ForEach(x => x.Visible = x == listControl);

        // Iterate over the fields of the Settings class and create corresponding UI nodes.
        var settingsType = type;
        var fields = settingsType.GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var settingAttribute = (SettingPropertyAttribute)Attribute.GetCustomAttribute(field, typeof(SettingPropertyAttribute));
            var convertedName = settingAttribute != null ? settingAttribute.Name : field.Name;

            BuildSetting(listControl, convertedName, obj, field.GetValue(obj), field.FieldType, field);
        }
    }

    public void BuildSetting(Control listControl, string name, object obj, object value, Type type, FieldInfo field)
    {
        // Create a label node to display the name of the field.
        var label = new Label { Text = name };

        // Create a VBoxContainer node to hold the UI setting nodes.
        var container = new HBoxContainer();
        container.AddChild(label);

        // Check the type of the field and create a corresponding UI node.
        if (type == typeof(double))
        {
            var rangeAttribute = (RangeSettingAttribute)Attribute.GetCustomAttribute(field, typeof(RangeSettingAttribute));
            if (rangeAttribute == null) GD.PushWarning("Range attribute is null, ignoring");
            var slider = new HSlider
            {
                MinValue = rangeAttribute != null ? rangeAttribute.Min : 0,
                MaxValue = rangeAttribute != null ? rangeAttribute.Max : 1,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
                Step = rangeAttribute != null ? rangeAttribute.Step : 0.1
            };
            slider.Value = (double)value;
            slider.ValueChanged += (value) => ValueChanged(value, obj, field);
            container.AddChild(slider);
        }
        else if (type == typeof(bool))
        {
            var checkBox = new CheckButton() { ButtonPressed = (bool)value };
            checkBox.Toggled += (value) => ValueChanged(value, obj, field);
            container.AddChild(checkBox);
            GD.Print($"CheckBox value: {checkBox.ButtonPressed}");
        }

        // Add the container node to the scene tree as a child of this node.
        listControl.AddChild(container);
    }

    // This function is called when any UI value is changed
    private void ValueChanged<T>(T value, object obj, FieldInfo field)
    {
        field.SetValue(obj, value);
        SettingsGlobals.UpdateSettings();
    }
}
