using Galatime.Global;
using Galatime.Interfaces;
using Godot;
using NodeExtensionMethods;

namespace Galatime
{
    public partial class ItemPickup : CharacterBody2D, ILevelObject
    {
        #region Exports
        /// <summary> Spawn velocity of the item pickup, which is used to move the item when spawned. </summary>
        [Export] public Vector2 SpawnVelocity;
        /// <summary> The item's id, which is used to get the item. </summary>
        [Export] public string ItemId;
        /// <summary> The item's quantity of the item pickup. </summary>
        [Export] public int Quantity = 1;
        #endregion

        #region Variables
        public Vector2 velocity = Vector2.Zero;
        public Item Item;
        #endregion

        #region Nodes
        public AnimationPlayer AnimationPlayer;
        public ItemContainer ItemContainer1;
        public ItemContainer ItemContainer2;
        public ItemContainer ItemContainer3;
        public GpuParticles2D Particles;
        public Area2D PickupArea;
        #endregion

        public override void _Ready()
        {
            #region Get nodes
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            ItemContainer1 = GetNode<ItemContainer>("Item1");
            ItemContainer2 = GetNode<ItemContainer>("Item2");
            ItemContainer3 = GetNode<ItemContainer>("Item3");
            Particles = GetNode<GpuParticles2D>("Particles");
            PickupArea = GetNode<Area2D>("PickupArea");
            #endregion

            PickupArea.BodyEntered += (Node2D node) => OnEntered(node);

            DisplayItem(ItemId);
            velocity = SpawnVelocity;

            LevelManager.Instance.SaveLevelObject(this, new object[] { GlobalPosition, false });
        }

        public void LoadLevelObject(object[] state)
        {
            var isPickedUp = (bool)state[1];
            if (isPickedUp) QueueFree();

            var pos = (Vector2)state[0];
            GlobalPosition = pos;
        }

        public void OnEntered(Node2D node)
        {
            if (node.IsPossessed())
            {
                GetNode<PlayerVariables>("/root/PlayerVariables").AddItem(Item, Quantity);
                LevelManager.Instance.SaveLevelObject(this, new object[] { GlobalPosition, true });
                QueueFree();
            }
        }

        public void DisplayItem(string id)
        {
            Item = GalatimeGlobals.GetItemById(id);
            ItemContainer1.DisplayItem(Item);
            if (Quantity >= 2 && Quantity < 5)
            {
                UpdateItemContainerPosition(ItemContainer1, -6);
                UpdateItemContainerPosition(ItemContainer2, 6);
                ItemContainer2.DisplayItem(Item);
                ItemContainer2.Visible = true;
            }
            if (Quantity >= 5)
            {
                UpdateItemContainerPosition(ItemContainer1, -6);
                UpdateItemContainerPosition(ItemContainer2, 6, -6);
                UpdateItemContainerPosition(ItemContainer3, 6, 6);
                ItemContainer2.DisplayItem(Item);
                ItemContainer2.Visible = true;
                ItemContainer3.DisplayItem(Item);
                ItemContainer3.Visible = true;
            }
        }
        
        private void UpdateItemContainerPosition(ItemContainer container, int deltaY = 0, int deltaX = 0)
        {
            var newPosition = container.Position;
            newPosition.Y += deltaY;
            newPosition.X += deltaX;
            container.Position = newPosition;
        }

        public override void _PhysicsProcess(double delta)
        {
            velocity = velocity.Lerp(Vector2.Zero, 0.02f);
            Particles.Emitting = velocity.Length() > 10;
            Velocity = velocity;
            MoveAndSlide();
        }
    }
}