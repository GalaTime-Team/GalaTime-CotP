using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Galatime;
public enum SlotType { COMMON, WEAPON };

public class Item
{
    /// <summary>
    /// The name of the item.
    /// </summary> 
    public string Name = "";

    /// <summary>
    /// The unique identificator of the item. MAKE IT UNIQUE.
    /// </summary>
    public string ID = "";

    /// <summary>
    /// The description of the item.
    /// </summary>
    public string Description = "";

    /// <summary>
    /// If the item is stackable in the inventory. Stacking - is the ability to add multiple items to the inventory in one slot.
    /// </summary>
    public bool Stackable;

    /// <summary>
    /// The stack size of the item. It means the number of items can be stacked.
    /// </summary>
    /// <remarks> It's not currently used. </remarks>
    public int StackSize = 1;

    /// <summary>
    /// The type of the item.
    /// </summary>
    public SlotType Type = SlotType.COMMON;

    /// <summary>
    /// The amount of the item.
    /// </summary> 
    public int Quantity = 0;

    private string iconPath = "";
    /// <summary>
    /// The path of the icon of the item.
    /// </summary>
    public string IconPath
    {
        get => iconPath;
        set
        {
            iconPath = value;
            if (iconPath != "")
            {
                GD.Print($"LOADING ITEM ICON: {iconPath}");
                Icon = GD.Load<Texture2D>(iconPath);
            }
        }
    }

    private string scenePath = "";
    /// <summary>
    /// The path of the scene of the item.
    /// </summary>
    public string ScenePath
    {
        get => scenePath;
        set
        {
            scenePath = value;
            if (scenePath != "")
            {
                GD.Print($"LOADING ITEM SCENE: {scenePath}");
                ItemScene = GD.Load<PackedScene>(scenePath);
            }
        }
    }

    /// <summary>
    /// The loaded scene of the item.
    /// </summary> 
    public PackedScene ItemScene;

    /// <summary>
    /// The loaded icon of the item.
    /// </summary> 
    public Texture2D Icon;

    /// <summary>
    /// If the item is empty checked by the <see cref="Quantity"/> and <see cref="ID"/>
    /// </summary>
    public bool IsEmpty
    {
        get => Quantity <= 0 && ID == "";
    }

    /// <summary>
    /// Create a new instance of the Item class. This prevents the item to being wrong referenced.
    /// </summary>
    /// <returns> A new instance of the Item class. </returns>
    public Item Clone() {
        return (Item)MemberwiseClone();
    }
}