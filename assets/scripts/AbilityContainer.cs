using Godot;
using System;

namespace Galatime
{
    public partial class AbilityContainer : Control
    {
        #region Nodes
        public Label ReloadLabel;
        public Label ChargesLabel;
        public TextureProgressBar ReloadProgressBar;
        public AnimationPlayer AnimationPlayer;
        public Timer ProgressBarTimer;
        public Timer TextTimer;
        #endregion

        #region Variables
        public float ReloadTime;
        public int MaxCharges = 1;
        private int charges;
        public int Charges {
            get => charges;
            set {
                charges = value;
                ChargesLabel.Text = MaxCharges > 1 ? $"{charges}/{MaxCharges}" : "";
            }
        }
        private float Remaining;
        private float Delay;
        private float ShakeAmount = 0;

        private AbilityData AbilityData;
        #endregion

        public Texture2D defaultTexture = GD.Load<Texture2D>("res://sprites/gui/abilities/empty.png");

        public override void _Ready()
        {
            #region Get nodes
            ReloadProgressBar = GetNode<TextureProgressBar>("ReloadProgressBar");
            ReloadLabel = GetNode<Label>("ReloadLabel");
            ChargesLabel = GetNode<Label>("ChargesContainer/ChargesLabel");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            #endregion

            #region Timers
            TextTimer = GetNode<Timer>("TextTimer");
            ProgressBarTimer = GetNode<Timer>("ClockTimer");
            TextTimer.Timeout += ProcessText;
            ProgressBarTimer.Timeout += ProcessProgressBar;
            #endregion
        }

        public override void _Process(double delta)
        {
            _Shake();
        }

        /// <summary>
        /// Creates a shake effect on the ability container.
        /// </summary>
        private void _Shake()
        {
            if (ShakeAmount > 0) return;
            var shakeOffset = new Vector2();

            Random rnd = new();
            shakeOffset.X = rnd.Next(-1, 2) * ShakeAmount;
            shakeOffset.Y = rnd.Next(-1, 2) * ShakeAmount + 10;

            ShakeAmount = Mathf.Lerp(ShakeAmount, 0, 0.05f);

            ReloadProgressBar.Position = shakeOffset;
        }

        /// <summary>
        /// Displays the ability data and loads it.
        /// </summary>
        /// <param name="data">The ability data to display</param>
        public void Load(AbilityData data)
        {
            AbilityData = data;

            StopReload();
            ReloadProgressBar.TextureUnder = data.Icon ?? defaultTexture;
            ReloadTime = data.Reload;
            MaxCharges = data.MaxCharges;
            Charges = data.Charges;

            // if (!data.IsFullyReloaded) 
            // {
            //     StartReload(data.Charges, (float)data.CooldownTimer.TimeLeft);
            // }
        }

        /// <summary>
        /// Unloads the ability data and displays the empty ability.
        /// </summary>
        public void Unload()
        {
            ReloadProgressBar.TextureUnder = defaultTexture;
            ReloadTime = 2;
            MaxCharges = 1;
            Charges = 1;
        }

        /// <summary>
        /// Starts the ability reload display. Call it once the ability is starting to reload. 
        /// </summary>
        public void StartReload(int charges, float reloadTime = 0)
        {
            GD.Print($"Starting reload with {charges} charges, {reloadTime} reload time");

            Charges = charges;
            // Don't reload if max charges is reached
            if (Charges >= AbilityData.MaxCharges) return;

            if (reloadTime <= 0) 
            {
                // Setting the clock timer to the reload time
                Delay = ReloadTime / 100;
                ProgressBarTimer.WaitTime = Delay;
                ReloadProgressBar.Value = 100;
                // Setting the label to the remaining time
                Remaining = ReloadTime;
            }
            else // If custom reload time is set, use it
            {
                Delay = reloadTime / 100;
                ProgressBarTimer.WaitTime = Delay;
                ReloadProgressBar.Value = reloadTime / 100;
                // Setting the label to the remaining time
                Remaining = reloadTime;
            }
            
            ReloadLabel.Text = Remaining + "s";

            // Starting the clock timer
            ProgressBarTimer.Start();
            TextTimer.Start();
        }

        public void StopReload()
        {
            ReloadLabel.Text = "";
            ReloadProgressBar.Value = 0;

            ProgressBarTimer.Stop();
            TextTimer.Stop();
        }

        /// <summary>
        /// Shows error animation. It will called when the ability is not able to be used.
        /// </summary>
        public void No()
        {
            ShakeAmount += 2;
            AnimationPlayer.Stop();
            AnimationPlayer.Play("no");
        }

        /// <summary>
        /// Shows click animation.
        /// </summary>
        public void Click()
        {
            ShakeAmount += 0.5f;
            AnimationPlayer.Stop();
            AnimationPlayer.Play("click");
        }

        private void ProcessProgressBar()
        {
            ReloadProgressBar.Value--;
            if (ReloadProgressBar.Value <= 0) ProgressBarTimer.Stop();
        }

        private void ProcessText()
        {
            Remaining--;
            ReloadLabel.Text = Remaining + "s";
            if (Remaining <= 0) { 
                TextTimer.Stop();
                ReloadLabel.Text = "";
            }
            if (ReloadProgressBar.Value <= 0) ReloadLabel.Text = "";
        }
    }
}
