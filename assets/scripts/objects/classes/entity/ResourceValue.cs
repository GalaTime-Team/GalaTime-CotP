using System;
using Godot;

public partial class ResourceValue : Node
{
    // The current value of the resource
    private float value;
    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float MaxValue;
    public float RegenValue;

    // The timer that starts the countdown before regenerating the resource
    public Timer CountdownTimer;
    // The timer that regenerates the resource periodically
    public Timer RegenTimer;
    // The delegate that handles the value changed event
    public Action<float> OnValueChanged;

    // The constructor that initializes the resource with the given values
    public ResourceValue(string name, float maxValue, float countdownTime, float regenTime, float value = -1, float regenValue = 10)
    {
        MaxValue = maxValue;
        RegenValue = regenValue;
        this.value = value == -1 ? maxValue : Math.Clamp(value, 0, maxValue);

        if (value != -1) SetValue(value);

        CountdownTimer = new Timer
        {
            WaitTime = countdownTime,
            OneShot = true,
            Name = name + "Countdown",
            Autostart = false
        };
        CountdownTimer.Timeout += () => RegenTimer.Start();

        RegenTimer = new Timer
        {
            WaitTime = regenTime,
            Name = name + "Regen",
            Autostart = false
        };
        RegenTimer.Timeout += Regen;
    }

    public override void _Ready()
    {
        AddChild(CountdownTimer);
        AddChild(RegenTimer);
    }

    /// <summary> The method that sets the value of the resource and clamps it between 0 and maxValue. </summary>
    /// <param name="justSet"> Whether or not to force to just set the value to not start the countdown timer and not invoke the OnValueChanged event </param>
    public void SetValue(float newValue, bool justSet = false)
    {
        value = (float)Math.Round(Math.Clamp(newValue, 0, MaxValue), 2);
        if (!justSet) 
        {
            CountdownTimer.Start();
            RegenTimer.Stop();
            OnValueChanged?.Invoke(value);
        }
    }

    // The method that regenerates the resource by a fixed amount
    public void Regen()
    {
        value += 10;
        value = Mathf.Clamp(value, 0, MaxValue);
        OnValueChanged?.Invoke(value);
        if (value >= MaxValue) RegenTimer.Stop();
    }
}
