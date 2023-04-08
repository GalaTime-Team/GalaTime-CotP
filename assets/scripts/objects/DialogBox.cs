using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using System.Net.Sockets;

public partial class DialogBox : NinePatchRect
{
    private string _dialogsPath = "res://assets/data/json/dialogs.json";
    private string _charactersPath = "res://assets/data/json/talking-characters.json";
    private RichTextLabel _textNode;    
    private Label _characterName;
    private TextureRect _characterPortrait;
    private AnimationPlayer _skipAnimationPlayer;
    private Timer _delay;
    private Player _player;
    private int currentPhrase = 0;
    private Godot.Collections.Array _dialog;
    public bool canSkip = false;

    private AudioStreamPlayer _dialogAudio;
    public override void _Ready()
    {
        _textNode = GetNode<RichTextLabel>("DialogText");
        _characterPortrait = GetNode<TextureRect>("CharacterPortrait");
        _dialogAudio = GetNode<AudioStreamPlayer>("Voice");
        _skipAnimationPlayer = GetNode<AnimationPlayer>("SkipAnimationPlayer");
        _characterName = GetNode<Label>("CharacterName");

        _delay = new Timer();
        _delay.WaitTime = 0.04f;
        _delay.OneShot = false;
        _delay.Timeout += () => _letterAppend();
        AddChild(_delay);
    }

    public Godot.Collections.Array _getDialogFromJSON(string id)
    {
        if (Godot.FileAccess.FileExists(_dialogsPath))
        {
            var file = Godot.FileAccess.Open(_dialogsPath, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            Godot.Collections.Array dialog = new Godot.Collections.Array();
            try
            {
                var dialogData = ((Godot.Collections.Dictionary)json.Data);
                dialog = (Godot.Collections.Array)dialogData[id];
            }
            catch (System.Exception e)
            {
                GD.PrintErr("DIALOG: Invalid dialog " + id + ". " + e.Message);
            }
            return dialog;
        }
        else
        {
            GD.PrintErr("DIALOG: Invalid path");
            return new Godot.Collections.Array();
        }
    }

    public Godot.Collections.Dictionary _getCharacterFromJSON(string name)
    {
        if (Godot.FileAccess.FileExists(_dialogsPath))
        {
            var file = Godot.FileAccess.Open(_charactersPath, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            Godot.Collections.Dictionary character = new Godot.Collections.Dictionary();
            try
            {
                var charactersData = ((Godot.Collections.Dictionary)json.Data);
                character = (Godot.Collections.Dictionary)charactersData[name];
            }
            catch (System.Exception e)
            {
                GD.PrintErr("DIALOG: Invalid character " + name + ". " + e.Message);
            }
            return character;
        }
        else
        {
            GD.PrintErr("DIALOG: Invalid path");
            return new Godot.Collections.Dictionary();
        }
    }

    public void startDialog(string id, Player p)
    {
        _player = p;

        _resetValues();
        _dialog = _getDialogFromJSON(id);

        if (_dialog != new Godot.Collections.Array())
        {
            _textNode.Text = "";
            Visible = true;
            canSkip = true;

            nextPhrase(currentPhrase);
        }
        else
        {
            GD.PrintErr("DIALOG: dialog " + id + " is not exist");
            endDialog();
        }
    }

    public void endDialog()
    {
        Visible = false;
        canSkip = false;
        _resetValues();
    }

    public string stripBBCode(string str)
    {
        var regex = new RegEx();
        regex.Compile("\\[.+?\\]");
        return regex.Sub(str, "", true);
    }

    public void _letterAppend()
    {
        // GD.Print("CurrentLetter: " + _textNode.VisibleCharacters + ". Current phrase: " + currentPhrase);
        try
        {
            if (_textNode.VisibleCharacters >= stripBBCode(_textNode.Text).Length) { _delay.Stop(); _skipAnimationPlayer.Play("loop"); canSkip = true; return; } else { _textNode.VisibleCharacters += 1; }
            _dialogAudio.Play();
        }
        catch (System.Exception)
        {

        }
    }

    public void startTyping() { _delay.Start(); }

    public void nextPhrase(int phraseId)
    {
        _skipAnimationPlayer.Play("start");
        canSkip = false;
        var phrase = (Godot.Collections.Dictionary)_dialog[phraseId];

        if (phrase.ContainsKey("actions"))
        {
            var actionsData = (Godot.Collections.Array)phrase["actions"];
            var action = (string)actionsData[0];
            actionsData.RemoveAt(0);
            var args = actionsData;
            if (args.Count != 0)    
            {
                GD.Print(action, args, "more arg");
                Callv(action, args);
            }
            else
            {
                GD.Print(action, args, "one arg");
                Call(action);
            }
        }
        else
        {
            GD.Print("dont actions");
        }

        if (phrase.ContainsKey("character") && phrase.ContainsKey("text"))
        {
            Godot.Collections.Dictionary character = _getCharacterFromJSON((string)phrase["character"]);

            _textNode.VisibleCharacters = 0;
            _textNode.Text = (string)phrase["text"];
            var emotion = (string)phrase["emotion"];
            Texture2D texture = GD.Load<Texture2D>((string)character[emotion]);
            _characterName.Text = (string)character["name"];
            if (texture is AnimatedTexture)
            {
                AnimatedTexture animatedTexture = (AnimatedTexture)texture;
                animatedTexture.CurrentFrame = 0;
                _characterPortrait.Texture = animatedTexture;
            }
            else
            {
                _characterPortrait.Texture = texture;
            }
            startTyping();
        }
        else
        {
            _player.EmitSignal("on_dialog_end");
            endDialog(); return;
        }

        GD.Print(phrase);
    }

    public void setCameraOffset(string x, string y)
    {
        _player.cameraOffset.X = int.Parse(x);
        _player.cameraOffset.Y = int.Parse(y);
    }

    public void toggleMove()
    {
        _player.canMove = !_player.canMove;
    }

    private void _resetValues()
    {
        _player.Set("can_move", true);
        _delay.Stop();

        _textNode.Text = "";
        _textNode.VisibleCharacters = -1;

        currentPhrase = 0;
        canSkip = false;
        _dialog = new Godot.Collections.Array();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            nextInput();
        }
    }

    public void nextInput()
    {
        if (canSkip)
        {
            if (currentPhrase + 1 >= _dialog.Count)
            {
                _player.EmitSignal("on_dialog_end");
                endDialog(); return;
            }
            else
            {
                currentPhrase += 1;
                nextPhrase(currentPhrase);
            }
        }
    }
}
