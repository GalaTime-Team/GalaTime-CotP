using Godot;

namespace Galatime
{
    public partial class DragPreview : Control
    {
        #region Nodes
        ItemContainer ItemContainer;
        AnimationPlayer AnimationPlayerStatus;
        #endregion

        private Item draggedItem = new();
        /// <summary> The item currently being dragged. When changed it will be displayed. </summary>
        public Item DraggedItem {
            get => draggedItem;
            set {
                draggedItem = value;
                ItemContainer.DisplayItem(draggedItem);
            }
        }

        public override void _Ready()
        {
            #region Get nodes
            ItemContainer = GetNode<ItemContainer>("Item");
            AnimationPlayerStatus = GetNode<AnimationPlayer>("AnimationPlayerStatus");
            #endregion
        }

        public override void _Process(double delta)
        {
            if (!draggedItem.IsEmpty)
            {
                Vector2 position = GetGlobalMousePosition();
                position.X -= 16;
                position.Y += 8;
                Position = position;
            }
        }

        public void Prevent()
        {
            AnimationPlayerStatus.Play("error");
        }
    }
}


