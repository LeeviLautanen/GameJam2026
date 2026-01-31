using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{
    [Export]
    Vector2 GuestMaskStartLocation = new(-200, 300);
    [Export]
    Vector2 GuestMaskStopLocation = new(600, 300);
    [Export]
    Vector2 GuestMaskEndLocation = new(1500, 300);
    [Export]
    public int NumberOfGuests = 3;
    [Export]
    float ScaleSmall = 2.0f;
    [Export]
    float StopWaitTime = 0.5f;
    [Signal]
    public delegate void GuestProcessedEventHandler(int guestNumber);

    private MaskManager referenceMask;
    private MaskManager guestMask;
    private bool[] maskDifferences;
    private int guestsProcessed = 0;
    private Tween moveTween;
    private bool submitPressed = false;


    public override void _Ready()
    {
        referenceMask = GetNode<MaskManager>("Mask");
        referenceMask.GenerateMask();
        maskDifferences = new bool[referenceMask.maskDetails.Count];
        _ = RunGuestCycle();
    }

    private async System.Threading.Tasks.Task RunGuestCycle()
    {
        guestMask = referenceMask.Duplicate() as MaskManager;
        AddChild(guestMask);

        Vector2 startingScale = guestMask.Scale;

        while (guestsProcessed < NumberOfGuests)
        {
            guestMask.Position = GuestMaskStartLocation;
            GD.Print(guestMask.Scale);
            Vector2 newSize = new Vector2(ScaleSmall, ScaleSmall);
            GD.Print(newSize);
            guestMask.Scale = newSize;
            guestMask.ShowMask();
            GenerateGuestMask();

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "position", GuestMaskStopLocation, 0.5f);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            await ToSignal(GetTree().CreateTimer(StopWaitTime), SceneTreeTimer.SignalName.Timeout);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "scale", startingScale, 0.5f);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            submitPressed = false;
            while (Array.Exists(maskDifferences, x => x) == true || submitPressed == false)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "scale", new Vector2(ScaleSmall, ScaleSmall), 0.5f);
            await ToSignal(moveTween, Tween.SignalName.Finished);

            moveTween = CreateTween();
            moveTween.TweenProperty(guestMask, "position", GuestMaskEndLocation, 0.5f);
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
