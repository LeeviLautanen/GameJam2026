using Godot;
using System.Collections.Generic;

public partial class MaskDetail : Sprite2D
{
    [Export]
    public string DetailName = "";
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

        int randomIndex = (int)rng.Randi() % Textures.Count;
        sprite2d.Texture = Textures[randomIndex];
        sprite2d.Visible = true;
        return randomIndex;
    }
}
