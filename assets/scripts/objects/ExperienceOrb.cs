using Godot;

using Galatime.Global;

using System.Collections.Generic;
using NodeExtensionMethods;

namespace Galatime
{
    public partial class ExperienceOrb : CharacterBody2D
    {
        #region Exports
        [Export] public int Quantity = 10;
        #endregion

        #region Nodes
        public Area2D PickupArea;
        private AnimationPlayer AnimationPlayer;
        private AnimationPlayer HUEanimationPlayer;
        private Sprite2D Sprite;
        private Player Player;
        #endregion

        /// <summary> The textures paths of the orb. </summary>
        private readonly string[] TexturesPath = new string[4] { 
            "res://assets/sprites/test/xp_orb_stage_0.png",
            "res://assets/sprites/test/xp_orb_stage_1.png", 
            "res://assets/sprites/test/xp_orb_stage_2.png", 
            "res://assets/sprites/test/xp_orb_stage_3.png" 
        };

        /// <summary> The loaded textures of the orb. </summary>
        private List<Texture2D> Textures = new();

        /// <summary> The thresholds of the orb, which determines the texture. </summary>
        public int[] TextureThresholds = { 10, 20, 40 };

        public override void _Ready()
        {
            // Loading the textures and adding them to the list.
            foreach (var texture in TexturesPath) Textures.Add(ResourceLoader.Load<Texture2D>(texture));

            #region Get nodes
            PickupArea = GetNode<Area2D>("PickupArea");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            HUEanimationPlayer = GetNode<AnimationPlayer>("HUEAnimationPlayer");
            Sprite = GetNode<Sprite2D>("Sprite2D");
            #endregion

            HUEanimationPlayer.Play("loop");

            Player = PlayerVariables.Instance.Player;

            PickupArea.BodyEntered += (Node2D node) => OnEntered(node);

            SetTexture();
        }

        private void SetTexture()
        {
            _ = TexturesPath[0];
            int index = 0;

            // Determining the texture based on the quantity.
            for (int i = 0; i < TextureThresholds.Length; i++) if (Quantity >= TextureThresholds[i]) index = i + 1;

            // Loading the texture.
            Sprite.Texture = GD.Load<Texture2D>(TexturesPath[index]);
        }

        public void OnEntered(Node2D node)
        {
            if (node.IsPossessed())
            {
                Player.Xp += Quantity;
                AnimationPlayer.Play("outro");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 vector = Vector2.Right;
            float rotation = GlobalPosition.AngleToPoint(Player.CurrentCharacter.Body.GlobalPosition);
            float distance = GlobalPosition.DistanceTo(Player.CurrentCharacter.Body.GlobalPosition);
            vector = vector.Rotated(rotation) * Mathf.Clamp(200 - distance, 20, 200);
            if (distance >= 5)
            {
                Velocity = vector;
                MoveAndSlide();
            }
        }
    }
}