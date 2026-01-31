using Godot;
using System.Collections.Generic;

public partial class MaskDetail : Sprite2D
{
    [Export]
    public string DetailName = "";
    [Export]
    public Godot.Collections.Array<Texture2D> Textures = new();

    private Sprite2D sprite2d;
    private RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        sprite2d = this;
    }

    public void HideDetail()
    {
        GD.Print("Hiding detail");
        sprite2d.Visible = false;
    }

    public int ShowRandomDetail()
    {
        if (Textures.Count == 0) return -1;

        rng.Randomize();
        int randomIndex = rng.RandiRange(0, Textures.Count - 1);
        sprite2d.Texture = Textures[randomIndex];
        sprite2d.Visible = true;
        return randomIndex;
    }
}
