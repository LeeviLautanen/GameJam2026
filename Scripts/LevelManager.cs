using Godot;
using System;
using System.Collections.Generic;

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

    public override void _Ready()
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

        screenSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        guestStartLocation = new Vector2(0 - 300, screenSize.Y);
        guestStopLocation = new Vector2(screenSize.X / 2, screenSize.Y);
        guestEndLocation = new Vector2(screenSize.X + 300, screenSize.Y);

        referenceMask.HideGuestSprite();
        referenceMask.GenerateReferenceMask();
        maskDifferences = new bool[referenceMask.maskDetails.Count];
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
            while (Array.Exists(maskDifferences, x => x) || !submitPressed)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

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
                maskDifferences[i] = false;
            }
            else
            {
                correctAnswers[i] = false;
            }
        }
        return correctAnswers;
    }
}
