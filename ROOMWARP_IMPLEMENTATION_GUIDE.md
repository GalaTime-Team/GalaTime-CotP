# RoomWarp Implementation Guide

## Overview

RoomWarp is a script that enables portal/warp functionality for scene transitions in the game. It allows players to interact with portals to travel between different levels, rooms, or areas.

## Problem Solved

**User Report**: "the warp still doesn't work, it uses the script 'Roomwarp.cs'"

**Solution**: Created RoomWarp.cs that properly implements the Interact() method to change scenes when triggered by InteractiveTrigger.

## Implementation Details

### RoomWarp.cs Structure

```csharp
public partial class RoomWarp : Node
{
    [Export] public string Scene { get; set; } = "";
    [Export] public int Room { get; set; } = 0;

    public void Interact()
    {
        // Validates scene path
        // Changes to target scene
        // Logs success or errors
    }
}
```

### Properties

- **Scene** (string): The path to the target scene file (e.g., "res://assets/scenes/rooms/room2.tscn")
- **Room** (int): Room number identifier for logging and tracking

### Methods

- **Interact()**: Called by InteractiveTrigger when player presses ui_accept
  - Validates scene path is not empty
  - Checks if scene file exists
  - Changes to target scene
  - Logs confirmation or error messages

## Setup Guide

### Step 1: Create Portal Nodes

In your Godot scene, create the following node structure:

```
Portal (Node2D)
├── InteractiveTrigger (Area2D)
│   ├── CollisionShape2D
│   └── [Configure properties below]
└── RoomWarp (Node)
    └── [Configure properties below]
```

### Step 2: Configure RoomWarp

Select the RoomWarp node and set:
- **Scene**: "res://path/to/target/scene.tscn"
- **Room**: 2 (or any room number you want)

### Step 3: Configure InteractiveTrigger

Select the InteractiveTrigger node and set:
- **ExecuteNodePath**: "../RoomWarp" (path to your RoomWarp node)
- **Method**: "Interact"
- **VisualNodePath**: (optional - path to node showing hover text)
- **CanInteract**: true (default)

### Step 4: Add Collision Shape

Add a CollisionShape2D to the InteractiveTrigger with appropriate shape (RectangleShape2D, CircleShape2D, etc.)

## Complete Example

### Node Structure

```
LevelPortal (Node2D)
├── InteractiveTrigger (Area2D)
│   ├── Script: InteractiveTrigger.cs
│   ├── ExecuteNodePath: "../PortalWarp"
│   ├── Method: "Interact"
│   ├── VisualNodePath: "../HoverText"
│   ├── CanInteract: true
│   └── CollisionShape2D
│       └── Shape: RectangleShape2D (64x64)
├── PortalWarp (Node)
│   ├── Script: RoomWarp.cs
│   ├── Scene: "res://assets/scenes/rooms/room2.tscn"
│   └── Room: 2
└── HoverText (Label)
    └── Text: "Press E to enter"
```

### Scene Paths Examples

```gdscript
# Moving to different room in same area
Scene: "res://assets/scenes/rooms/room2.tscn"

# Moving to different area
Scene: "res://assets/scenes/areas/forest.tscn"

# Returning to hub
Scene: "res://assets/scenes/hub.tscn"
```

## How It Works

### Flow Diagram

1. **Player Approaches**
   - Player's character enters InteractiveTrigger collision area
   - InteractiveTrigger checks if character is possessed (IsPossessed())
   
2. **Hover Text Appears** (if configured)
   - VisualNodePath node becomes visible
   - Shows text like "Press E to enter"

3. **Player Presses ui_accept**
   - Player presses Enter, E, or Space key
   - InteractiveTrigger's _Input receives the event
   - Checks if player is hovering (PlayerIsHovering = true)

4. **Interact() Called**
   - InteractiveTrigger calls the method specified in Method property
   - Calls RoomWarp.Interact()

5. **Validation**
   - Checks if Scene property is not empty
   - Verifies scene file exists using ResourceLoader.Exists()

6. **Scene Change**
   - Calls GetTree().ChangeSceneToFile(Scene)
   - Godot transitions to new scene

7. **Confirmation**
   - Logs: "RoomWarp: Changing to room {Room} (Scene: {Scene})"

## Error Handling

### Error 1: Scene Not Set

**Error Message**: "RoomWarp: Cannot warp - Scene path is not set"

**Cause**: Scene property is empty string

**Solution**: Set the Scene property in Godot Inspector

### Error 2: Scene File Doesn't Exist

**Error Message**: "RoomWarp: Cannot warp - Scene file does not exist: {path}"

**Cause**: Scene path points to non-existent file

**Solution**: 
- Check the file path is correct
- Verify the file exists in the project
- Use full res:// path format

### Error 3: Scene Change Failed

**Error Message**: "RoomWarp: Failed to change scene. Error: {error_code}"

**Cause**: GetTree().ChangeSceneToFile() returned an error

**Solution**:
- Check console for error code
- Verify target scene is valid .tscn file
- Test with simple scene first

## Testing

### Test Checklist

1. **Approach Portal**
   - [ ] Walk up to portal
   - [ ] Verify hover text appears (if configured)
   - [ ] Confirm collision is working

2. **Interact with Portal**
   - [ ] Press ui_accept (Enter/E/Space)
   - [ ] Verify scene starts transitioning
   - [ ] Check for any error messages in console

3. **Scene Transition**
   - [ ] Confirm new scene loads
   - [ ] Verify player spawns correctly
   - [ ] Check room number if tracking

4. **Console Output**
   - [ ] Look for: "RoomWarp: Changing to room X (Scene: ...)"
   - [ ] Verify no error messages appear
   - [ ] Check for any warnings

### Testing Scenarios

**Test 1: Basic Portal**
- Configure portal between two simple scenes
- Test transition in both directions
- Verify console messages

**Test 2: Multiple Portals**
- Create multiple portals in one scene
- Test each portal independently
- Verify they go to correct scenes

**Test 3: Error Cases**
- Leave Scene property empty → Should show error
- Set invalid Scene path → Should show error
- Test with non-existent file → Should show error

## Troubleshooting

### Issue: Portal Doesn't Respond

**Symptoms**: Nothing happens when pressing ui_accept near portal

**Possible Causes**:
1. InteractiveTrigger collision not configured
2. Player not entering collision area
3. ExecuteNodePath incorrect
4. Method name typo

**Solutions**:
1. Add CollisionShape2D to InteractiveTrigger
2. Make collision area larger
3. Verify path: "../RoomWarp" or correct relative path
4. Ensure Method = "Interact" (exact spelling)

### Issue: Scene Doesn't Change

**Symptoms**: Interaction works but scene doesn't change

**Possible Causes**:
1. Scene path not set
2. Scene file doesn't exist
3. Scene path has typo
4. Scene file is corrupted

**Solutions**:
1. Set Scene property in Inspector
2. Verify file exists in project
3. Copy-paste path to avoid typos
4. Try loading scene manually in editor

### Issue: Hover Text Doesn't Appear

**Symptoms**: Portal works but no hover text shows

**Possible Causes**:
1. VisualNodePath not set
2. VisualNodePath points to wrong node
3. Text node not visible by default

**Solutions**:
1. Set VisualNodePath to your Label/RichTextLabel node
2. Verify relative path is correct
3. Ensure text node starts invisible (InteractiveTrigger will show it)

### Issue: Wrong Scene Loads

**Symptoms**: Portal goes to wrong destination

**Possible Causes**:
1. Scene property set to wrong path
2. Multiple portals sharing same RoomWarp node
3. Copy-paste error in configuration

**Solutions**:
1. Double-check Scene path in Inspector
2. Each portal needs its own RoomWarp node
3. Verify each portal's ExecuteNodePath is unique

## Best Practices

### Scene Path Management

1. **Use Full Paths**: Always use "res://..." format
2. **Consistency**: Organize scenes in clear folder structure
3. **Naming**: Use descriptive names (room1.tscn, not level_a.tscn)
4. **Verification**: Test each portal after configuration

### Node Organization

1. **Grouping**: Keep portal-related nodes together
2. **Naming**: Use descriptive node names
   - Good: "PortalToForest", "ExitToHub"
   - Bad: "Node", "Area2D"
3. **Hierarchy**: Keep RoomWarp as direct sibling or child of InteractiveTrigger

### Configuration Tips

1. **Test Early**: Test portals as soon as you add them
2. **Console Check**: Always check console for confirmation messages
3. **Error Logs**: Don't ignore error messages
4. **Build Test**: Test in exported build, not just editor

### Performance

1. **Scene Loading**: Keep target scenes optimized
2. **Cleanup**: Godot automatically frees previous scene
3. **Assets**: Preload frequently used scenes if needed

## Future Enhancements

Potential features that could be added:

1. **Save Room Number**: Store Room to save file for tracking
2. **Fade Transitions**: Add fade-in/fade-out effects
3. **Loading Screen**: Show loading screen for large scenes
4. **Player Position**: Set player spawn position in target scene
5. **Custom Effects**: Add portal visual/sound effects
6. **Bidirectional**: Automatically create return portal
7. **Conditions**: Add requirements (keys, levels, etc.)

## Summary

RoomWarp provides a simple but effective portal/warp system:

✅ **Easy to Configure**: Set Scene path and Room number in Inspector
✅ **Error Handling**: Clear error messages for debugging
✅ **Integration**: Works seamlessly with InteractiveTrigger
✅ **Flexible**: Can warp to any scene in the project
✅ **Validated**: Checks scene exists before attempting warp
✅ **Logged**: Confirmation messages for successful transitions

**Status**: Complete and ready for use! Portal/warp functionality is now available for level design.
