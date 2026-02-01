using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{
    [Export]
    public int NumberOfGuests = 3;
    [Export]
    public float ScaleSmall = 1.5f;
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
    private float scaleBig = 3.0f;

    public override void _Ready()
    {
        referenceMask = GetNode<MaskManager>("Mask");
        if (referenceMask == null)
        {
            GD.PrintErr("Reference MaskManager not found in the scene tree.");
            return;
        }

        cameraController = GetNode<CameraController>("Camera2D");
        if (cameraController == null)
        {
            GD.PrintErr("CameraController not found in the scene tree.");
            return;
        }

        screenSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        guestStartLocation = new Vector2(0 - 300, screenSize.Y);
        guestStopLocation = new Vector2(screenSize.X / 2, screenSize.Y);
        guestEndLocation = new Vector2(screenSize.X + 300, screenSize.Y);

        referenceMask.HideGuestSprite();
        referenceMask.GenerateMask();
        maskDifferences = new bool[referenceMask.maskDetails.Count];
        _ = RunGuestCycle();
    }

    private async System.Threading.Tasks.Task RunGuestCycle()
    {
        guestMask = referenceMask.Duplicate() as MaskManager;
        AddChild(guestMask);
        guestMask.maskDetails = guestMask.GetMaskDetails();
        guestMask.ShowGuestSprite();

        while (guestsProcessed < NumberOfGuests)
        {
            guestMask.Position = guestStartLocation;
            guestMask.Scale = new(ScaleSmall, ScaleSmall);
            guestMask.ShowMask();
            GenerateGuestMask();

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "position", guestStopLocation, guestMoveSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            await ToSignal(GetTree().CreateTimer(StopWaitTime), SceneTreeTimer.SignalName.Timeout);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "scale", new Vector2(scaleBig, scaleBig), guestZoomSpeed)
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
            moveTween.TweenProperty(guestMask, "scale", new Vector2(ScaleSmall, ScaleSmall), guestZoomSpeed)
                     .SetTrans(Tween.TransitionType.Sine)
                     .SetEase(Tween.EaseType.InOut);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "position", guestEndLocation, guestMoveSpeed)
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
        guestMask.GenerateMask();

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

    public bool DifferenceSubmitted(int detailIndex)
    {
        submitPressed = true;

        if (detailIndex < 0 || detailIndex >= maskDifferences.Length)
        {
            return false;
        }

        if (maskDifferences[detailIndex] == true)
        {
            maskDifferences[detailIndex] = false;
            return true;
        }

        GD.Print("Incorrect selection at index: " + detailIndex);
        return false;
    }
}
