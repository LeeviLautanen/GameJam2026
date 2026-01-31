using Godot;
using System;
using System.Threading.Tasks;

public partial class CameraController : Camera2D
{
    [Export] public float MoveDuration = 0.5f;
    public bool BlockInput = false;

    private Vector2I screenSize;
    private Vector2 upPosition;
    private Vector2 downPosition;

    public override void _Ready()
    {
        screenSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        upPosition = new(0, 0);
        downPosition = new(0, screenSize.Y);

        GlobalPosition = upPosition;
    }

    public override void _Process(double delta)
    {
        if (BlockInput) return;

        if (Input.IsActionJustPressed("MoveDown"))
            MoveDown();
        else if (Input.IsActionJustPressed("MoveUp"))
            MoveUp();
    }

    public void MoveUp()
    {
        AnimateToPosition(upPosition);
    }

    public void MoveDown()
    {
        AnimateToPosition(downPosition);
    }

    private async void AnimateToPosition(Vector2 target)
    {
        BlockInput = true;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "global_position", target, MoveDuration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, Tween.SignalName.Finished);

        BlockInput = false;
    }
}
