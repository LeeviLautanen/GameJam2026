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
    private bool isMoving = false;
    private Sprite2D red1;
    private Sprite2D red2;
    private Sprite2D red3;

    public override void _Ready()
    {
        screenSize = (Vector2I)GetViewport().GetVisibleRect().Size;

        upPosition = new(0, 0);
        downPosition = new(0, screenSize.Y);

        GlobalPosition = upPosition;

        red1 = GetNode<Sprite2D>("XRed");
        red2 = GetNode<Sprite2D>("XRed2");
        red3 = GetNode<Sprite2D>("XRed3");
    }

    public override void _Process(double delta)
    {
        if (BlockInput || isMoving) return;

        if (Input.IsActionJustPressed("MoveDown"))
            MoveDown();
        else if (Input.IsActionJustPressed("MoveUp"))
            MoveUp();
    }

    public void SetHealth(int health)
    {
        switch (health)
        {
            case 3:
                red1.Visible = false;
                red2.Visible = false;
                red3.Visible = false;
                break;
            case 2:
                red1.Visible = true;
                red2.Visible = false;
                red3.Visible = false;
                break;
            case 1:
                red1.Visible = true;
                red2.Visible = true;
                red3.Visible = false;
                break;
            case 0:
                red1.Visible = true;
                red2.Visible = true;
                red3.Visible = true;
                break;
            default:
                break;
        }
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
        isMoving = true;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "global_position", target, MoveDuration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, Tween.SignalName.Finished);

        isMoving = false;
    }
}
