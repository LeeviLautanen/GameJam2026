using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelManager : Node
{
    [Export]
    public int NumberOfGuests = 3;
    [Signal]
    public delegate void GuestProcessedEventHandler(int guestNumber);

    private MaskManager referenceMask;
    private MaskManager guestMask;
    private bool[] maskDifferences;
    private int guestsProcessed = 0;
    private Tween moveTween;
    private bool submitPressed = false;
    private CameraController cameraController;
    private Vector2I screenSize;
    private Vector2 guestStartLocation;
    private Vector2 guestStopLocation;
    private Vector2 guestEndLocation;
    private float StopWaitTime = 0.3f;
    private float guestMoveSpeed = 1.0f;
    private float guestZoomSpeed = 0.5f;
    private float scaleSmall = 0.7f;
    private float scaleBig = 1.0f;
    private int health = 3;

    public override async void _Ready()
    {
        referenceMask = GetNode<MaskManager>("Guest/Mask");
        if (referenceMask == null)
        {
            GD.PrintErr("LevelManager didnt find the reference mask");
            return;
        }

        cameraController = GetNode<CameraController>("Camera2D");
        if (cameraController == null)
        {
            GD.PrintErr("LevelManager didnt find the camera");
            return;
        }

        cameraController.SetHealth(health);
        screenSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        guestStartLocation = new Vector2(0 - 300, screenSize.Y);
        guestStopLocation = new Vector2(screenSize.X / 2, screenSize.Y);
        guestEndLocation = new Vector2(screenSize.X + 300, screenSize.Y);

        referenceMask.HideGuestSprite();
        referenceMask.GenerateReferenceMask();
        maskDifferences = new bool[referenceMask.maskDetails.Count];
        CallDeferred(nameof(StartLevel));
    }

    private async void StartLevel()
    {
        await FadeFromBlack();
        _ = RunGuestCycle();
    }


    private async System.Threading.Tasks.Task RunGuestCycle()
    {
        Node2D guestNode = referenceMask.GetParent().Duplicate() as Node2D;
        AddChild(guestNode);
        guestMask = guestNode.GetNode<MaskManager>("Mask");
        guestMask.ShowGuestSprite();
        GD.Print("Starting guest " + (guestsProcessed + 1));

        while (guestsProcessed < NumberOfGuests)
        {
            guestNode.Position = guestStartLocation;
            guestNode.Scale = new(scaleSmall, scaleSmall);
            guestMask.ShowMask();
            GenerateGuestMask();

            moveTween = CreateTween();
            moveTween.TweenProperty(guestNode, "position", guestStopLocation, guestMoveSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            await ToSignal(GetTree().CreateTimer(StopWaitTime), SceneTreeTimer.SignalName.Timeout);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestNode, "scale", new Vector2(scaleBig, scaleBig), guestZoomSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            cameraController.BlockInput = false;

            submitPressed = false;
            while (maskDifferences.Any(x => x) || !submitPressed)
            {
                if (health <= 0)
                {
                    await FadeAndSwitchScene("Level2.tscn");
                    return;
                }

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }


            cameraController.BlockInput = true;
            cameraController.MoveUp();

            moveTween = CreateTween();
            moveTween.TweenProperty(guestNode, "scale", new Vector2(scaleSmall, scaleSmall), guestZoomSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestNode, "position", guestEndLocation, guestMoveSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            guestMask.HideMask();
            guestsProcessed++;
            EmitSignal("GuestProcessed", guestsProcessed);
        }

        switch (Name)
        {
            case "Level1":
                GetTree().ChangeSceneToFile("res://Level3.tscn");
                break;
            case "Level2":
                GetTree().ChangeSceneToFile("res://StartMenu.tscn");
                break;
            default:
                break;
        }
    }

    public void GenerateGuestMask()
    {
        guestMask.GenerateMask(referenceMask.MaskData);

        int[] referenceData = referenceMask.MaskData;
        int[] guestData = guestMask.MaskData;

        if (referenceData == null || guestData == null ||
            referenceData.Length != guestData.Length)
        {
            GD.PrintErr("Mask data arrays are invalid or different lengths!");
            return;
        }

        for (int i = 0; i < referenceData.Length; i++)
        {
            maskDifferences[i] = referenceData[i] != guestData[i];
        }
    }

    private async System.Threading.Tasks.Task FadeAndSwitchScene(string scenePath)
    {
        ColorRect fade = new ColorRect();
        AddChild(fade);
        fade.Size = new Vector2(screenSize.X, screenSize.Y * 3);
        fade.Color = new Color(0, 0, 0, 0);
        fade.ZIndex = 100;

        Tween tween = CreateTween();
        tween.TweenProperty(fade, "color:a", 1.0f, 1.0f);
        await ToSignal(tween, Tween.SignalName.Finished);

        if (string.IsNullOrEmpty(scenePath))
            GetTree().ReloadCurrentScene();
        else
            GetTree().ChangeSceneToFile(scenePath);
    }

    private async System.Threading.Tasks.Task FadeFromBlack()
    {
        ColorRect fade = new ColorRect();
        AddChild(fade);
        fade.Size = new Vector2(screenSize.X, screenSize.Y * 3);
        fade.Color = new Color(0, 0, 0, 1);
        fade.ZIndex = 100;

        Tween tween = CreateTween();
        tween.TweenProperty(fade, "color:a", 0.0f, 1.0f);
        await ToSignal(tween, Tween.SignalName.Finished);

        fade.QueueFree();
    }


    public bool[] DifferenceSubmitted(bool[] selectedDetails)
    {
        submitPressed = true;

        if (selectedDetails.Length != maskDifferences.Length)
        {
            GD.PrintErr("Submitted details length does not match mask differences length!");
            return [];
        }

        bool[] correctAnswers = new bool[maskDifferences.Length];
        for (int i = 0; i < maskDifferences.Length; i++)
        {
            GD.Print($"Mask Difference: {maskDifferences[i]}, Selected: {selectedDetails[i]}");
            if ((maskDifferences[i] && selectedDetails[i]) || (!maskDifferences[i] && !selectedDetails[i]))
            {
                correctAnswers[i] = true;
                //maskDifferences[i] = false;
            }
            else
            {
                correctAnswers[i] = false;
            }
        }

        if (selectedDetails.All(x => x == false) && maskDifferences.Any(x => x))
        {
            GD.Print("No differences selected but differences exist.");
            health--;
            cameraController.SetHealth(health);
            return correctAnswers;
        }

        for (int i = 0; i < maskDifferences.Length; i++)
        {
            if (!correctAnswers[i] && selectedDetails[i])
            {
                GD.Print("Some selected answers were incorrect.");
                health--;
                cameraController.SetHealth(health);
                return correctAnswers;
            }
        }

        for (int i = 0; i < maskDifferences.Length; i++)
        {
            if (correctAnswers[i] && selectedDetails[i])
            {
                GD.Print("Correct answer cleared at " + i);
                maskDifferences[i] = false;
            }
        }

        return correctAnswers;
    }
}
