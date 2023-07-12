using Godot;
using System;
using Galatime;
using System.Collections.Generic;

namespace Galatime
{
	public partial class Player : HumanoidCharacter
	{
		// Exports
		[Export] public bool canInteract = true;
		[Export] public Vector2 cameraOffset;   
		public float cameraShakeAmount = 0;

		// Variables
		private int slots = 16;
		private int xp;
		public int Xp
		{
			get { return xp; }
			set
			{
				xp = value;
				playerGui.changeStats(Stats, xp);
				PlayerVariables.invokeXpChangedEvent(xp);
			}
		}

		public PlayerGui playerGui;

		private bool _isPause = false;

		// Nodes
		private CharacterBody2D _body;

		private Camera2D _camera;

		private RichTextLabel _debug;

		private PlayerVariables _playerVariables;

		// Signals
		[Signal] public delegate void on_pauseEventHandler(bool visible);
		[Signal] public delegate void healthChangedEventHandler(float health);
		[Signal] public delegate void on_interactEventHandler();
		[Signal] public delegate void on_dialog_endEventHandler();
		[Signal] public delegate void reloadDodgeEventHandler();

		public override void _Ready()
		{
			base._Ready();

			// Get Nodes
			_animationPlayer = GetNode<AnimationPlayer>("Animation");

			body = this;

			weapon = GetNode<Hand>("Hand");

			_camera = GetNode<Camera2D>("Camera3D");

			_sprite = GetNode<Sprite2D>("Sprite2D");
			_trailParticles = GetNode<GpuParticles2D>("TrailParticles");

			_playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
			_playerVariables.Connect("items_changed", new Callable(this, "_onItemsChanged"));
			_playerVariables.Connect("abilities_changed", new Callable(this, "_abilitiesChanged"));

			var playerGuiScene = ResourceLoader.Load<PackedScene>("res://assets/objects/PlayerGui.tscn");
		    playerGui = playerGuiScene.Instantiate<PlayerGui>();
			GetNode("CanvasLayer").AddChild(playerGui);
			playerGui.RequestReady();

			element = GalatimeElement.Ignis + GalatimeElement.Chaos;

			Stats = new EntityStats(
				physicalAttack: 75,
				magicalAttack: 80,
				physicalDefence: 65,
				magicalDefence: 75,
				health: 70,
				mana: 65,
				stamina: 65,
				agility: 60
			);

			// Start
			canMove = true;

			Stamina = Stats[EntityStatType.stamina].Value;
			Mana = Stats[EntityStatType.mana].Value;
			Health = Stats[EntityStatType.health].Value;

			cameraOffset = Vector2.Zero;

			body.GlobalPosition = GlobalPosition;

			playerGui.changeStats(Stats, Xp);

			Stats.statsChanged += _onStatsChanged;

			GD.Print("PLAYER INSTANCE");
			_playerVariables.setPlayerInstance(this);
		}

		private void _onStatsChanged(EntityStats stats)
		{
			GD.Print("PLAYER STATS CHANGED!!!");
			Health = Stats[EntityStatType.health].Value;
			Mana = Stats[EntityStatType.mana].Value;
			Stamina = Stats[EntityStatType.stamina].Value;

			playerGui._onStatsChanged(stats);
		}

		private void _SetMove()
		{
			Vector2 inputVelocity = Vector2.Zero;
			// Vector2 windowPosition = OS.WindowPosition;

			if (Input.IsActionPressed("game_move_up"))
			{
				inputVelocity.Y -= 1;
				// windowPosition.Y -= 1;
			}
			if (Input.IsActionPressed("game_move_down"))
			{
				inputVelocity.Y += 1;
				// windowPosition.Y += 1;
			}
			if (Input.IsActionPressed("game_move_right"))
			{
				inputVelocity.X += 1;
				// windowPosition.x += 1;
			}   
			if (Input.IsActionPressed("game_move_left"))
			{
				inputVelocity.X -= 1;
				// windowPosition.x -= 1;
			}
		   inputVelocity = inputVelocity.Normalized() * speed;

			// OS.WindowPosition = windowPosition;
			if (canMove && !_isDodge) velocity = inputVelocity; else velocity = Vector2.Zero;

			weapon.LookAt(GetGlobalMousePosition());
			_SetAnimation(_vectorRotation, velocity.Length() == 0 ? true : false);
			_setCameraPosition();
		}

		private void _setCameraPosition()
		{
			_camera.GlobalPosition = _camera.GlobalPosition.Lerp((weapon.GlobalPosition + (GetGlobalMousePosition() - weapon.GlobalPosition) / 5) + cameraOffset, 0.05f);
		}

		public override void _moveProcess()
		{
			if (!_isPause) _SetMove(); else velocity = Vector2.Zero;
			var shakeOffset = new Vector2();

			Random rnd = new();
			shakeOffset.X = rnd.Next(-1, 1) * cameraShakeAmount;
			shakeOffset.Y = rnd.Next(-1, 1) * cameraShakeAmount;

			_camera.Offset = shakeOffset;

			cameraShakeAmount = Mathf.Lerp(cameraShakeAmount, 0, 0.05f);
		}

		public override void _healthChangedEvent(float health)
		{
			playerGui.onHealthChanged(health);
		}

		protected override void _onManaChanged(float mana)
		{
			playerGui.onManaChanged(mana);
			GD.Print("MANA CHANGED");
		}

		protected override void _onStaminaChanged(float stamina)
		{
			playerGui.onStaminaChanged(stamina);
            GD.Print("STAMINA CHANGED");
        }

		public void _abilitiesChanged()
		{
			var obj = (Godot.Collections.Dictionary)_playerVariables.abilities;
			for (int i = 0; i < obj.Count; i++)
			{
				var ability = (Godot.Collections.Dictionary)obj[i];
				if (ability.ContainsKey("path"))
				{
					var existAbility = _abilities[i];
					if (existAbility != null)
					{
						if ((string)ability["path"] != existAbility.ResourcePath) addAbility((string)ability["path"], i);
					}
					else
					{
						addAbility((string)ability["path"], i);
					}
				}
				else
				{
					removeAbility(i);
					GD.Print("Ability: no path given at " + i);
				}
			}
		}

		public override GalatimeAbility addAbility(string scenePath, int i)
		{
			var ability = base.addAbility(scenePath, i);
			playerGui.addAbility(ability, i);
			return ability;
		}

		protected override bool _useAbility(int i)
		{
			var result = base._useAbility(i);
			if (!result)
			{
				playerGui.pleaseSayNoToAbility(i);
				return result;
			}
			playerGui.reloadAbility(i);
			return result;
		}

		protected override void removeAbility(int i)
		{
			base.removeAbility(i);
			playerGui.removeAbility(i);
		}

		public override void _Process(double delta)
		{
			// _debug.Text = $"hp {health} stamina {stamina} mana {mana} element {element.name}";
		}

		private void _onItemsChanged()
		{
			var obj = (Godot.Collections.Dictionary)_playerVariables.inventory[0];
			if (weapon._item != null && obj.Count != 0) return;
			weapon.takeItem(obj);
		}

		public void startDialog(string id)
		{
			playerGui.startDialog(id, this);
		}

		public override async void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventMouseMotion)
			{
				setDirectionByWeapon();
			}
			if (@event.IsActionPressed("ui_cancel"))
			{
				if (_isPause)
				{
					_isPause = false;
					playerGui.pause(_isPause);
					return;
				}
				if (!_isPause)
				{
					_isPause = true;
					playerGui.pause(_isPause);
					return;
				}
			}
			if (@event.IsActionPressed("game_attack"))
			{
				weapon.attack(Stats[EntityStatType.physicalAttack].Value, Stats[EntityStatType.magicalAttack].Value);
			}

			if (@event.IsActionPressed("game_dodge"))
			{
				dodge();
				var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
				globals.save(PlayerVariables.currentSave, playerGui);
			}

			if (@event.IsActionPressed("game_ability_1")) _useAbility(0);
			if (@event.IsActionPressed("game_ability_2")) _useAbility(1);
			if (@event.IsActionPressed("game_ability_3")) _useAbility(2);

			if (@event.IsActionPressed("ui_accept"))
			{
				if (canInteract)
				{
					EmitSignal("on_interact");
				}
			}
		}
	}
}
