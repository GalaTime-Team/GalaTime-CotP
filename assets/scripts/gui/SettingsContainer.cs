using Galatime.Global;
using Galatime.Settings;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Galatime.UI;

public partial class SettingsContainer : Control
{
    public const string ActionButtonScenePath = "res://assets/objects/gui/settings/ActionButton.tscn";

    public ScrollContainer SettingsListContainer;
    private SettingsGlobals SettingsGlobals;

    /// <summary> The first control when UI is builded. </summary>
    public Control FirstControl;

    public VBoxContainer CategoriesList;
    public LabelButton CategoryButton;
    public PackedScene ActionButtonScene;

    public List<Control> CategoryControls = new();
    public List<LabelButton> CategoryButtons = new();

    public SettingsData SettingsData;

    public override void _Ready()
    {
        SettingsListContainer = GetNode<ScrollContainer>("ScrollContainer");
        SettingsGlobals = GetNode<SettingsGlobals>("/root/SettingsGlobals");
        ActionButtonScene = GD.Load<PackedScene>(ActionButtonScenePath);

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
        CategoryButtonPressed(CategoryControls[0]);
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
        categoryButton.Pressed += () => CategoryButtonPressed(listControl);

        // Iterate over the fields of the Settings class and create corresponding UI nodes.
        var settingsType = type;
        var fields = settingsType.GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var settingAttribute = (SettingPropertyAttribute)Attribute.GetCustomAttribute(field, typeof(SettingPropertyAttribute));

            BuildSetting(listControl, settingAttribute, obj, field.GetValue(obj), field.FieldType, field);
        }
    }

    public void CategoryButtonPressed(Control listControl) => CategoryControls.ForEach(x => x.Visible = x == listControl);
    public static Attribute GetAttribute(FieldInfo field) => Attribute.GetCustomAttribute(field, typeof(SettingPropertyAttribute));
    public static T GetAttribute<T>(FieldInfo field) where T : Attribute => (T)Attribute.GetCustomAttribute(field, typeof(T));

    public void BuildSetting(Control listControl, SettingPropertyAttribute attribute, object obj, object value, Type type, FieldInfo field)
    {
        var convertedName = attribute != null ? attribute.Name : field.Name;

        // Create a setting element in the UI with the name of the setting.
        var settingElement = AssetsManager.Instance.GetSceneAsset<HBoxContainer>("setting_element");
        var settingName = settingElement.GetNode<Label>("Label");
        settingName.Text = convertedName;

        // Check the type of the field and create a corresponding UI node.
        if (type == typeof(double))
        {
            var rangeAttribute = GetAttribute<RangeSettingAttribute>(field);
            if (rangeAttribute == null) GD.PushWarning("Range attribute is null, ignoring");
            var slider = new HSlider
            {
                MinValue = rangeAttribute != null ? rangeAttribute.Min : 0,
                MaxValue = rangeAttribute != null ? rangeAttribute.Max : 1,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
                Step = rangeAttribute != null ? rangeAttribute.Step : 0.05
            };
            slider.Value = (double)value;
            slider.ValueChanged += (value) => ValueChanged(value, obj, field);
            settingElement.AddChild(slider);
        }
        else if (type == typeof(bool))
        {
            var checkBox = new CheckButton() { ButtonPressed = (bool)value, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            checkBox.Toggled += (value) => ValueChanged(value, obj, field);
            settingElement.AddChild(checkBox);
        }
        else if (type == typeof(long))
        {
            var keybindAttribute = GetAttribute<KeybindSettingAttribute>(field);
            if (keybindAttribute != null)
            {
                var actionButton = ActionButtonScene.Instantiate<ActionButton>();

                actionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                actionButton.ActionName = keybindAttribute.ActionName;
                actionButton.Key = (long)value;
                actionButton.OnBound += (long key) => ValueChanged(key, obj, field);

                // Add the container node to the scene tree as a child of this node.
                settingElement.AddChild(actionButton);
            }
        }
        else if (type == typeof(string))
        {
            var optionsAttribute = GetAttribute<OptionsSettingAttribute>(field);
            var val = (string)value;
            var menuButton = new MenuButton
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                Flat = false,
                Text = val,
                FocusMode = FocusModeEnum.All
            };
            var popup = menuButton.GetPopup();
            foreach (var option in optionsAttribute.Names)
            {
                popup.AddItem(option);
            }
            popup.SetFocusedItem(optionsAttribute.Names.ToList().FindIndex(x => x == val));
            popup.IndexPressed += (index) =>
            {
                menuButton.Text = optionsAttribute.Names[index];
                ValueChanged(optionsAttribute.Names[index], obj, field);
            };
            settingElement.AddChild(menuButton);
        }

        listControl.GetChild(0).AddChild(settingElement);
    }

    // This function is called when any UI value is changed
    private void ValueChanged<T>(T value, object obj, FieldInfo field)
    {
        // GD.Print($"Value is changed {value}");
        field.SetValue(obj, value);
        SettingsGlobals.UpdateSettings();
    }
}