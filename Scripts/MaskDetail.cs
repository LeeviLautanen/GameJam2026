using Godot;
using System.Collections.Generic;

public partial class MaskDetail : Sprite2D
{
    [Export]
    public string DetailName = "";
    [Export]
    public Godot.Collections.Array<Texture2D> Textures = new();

    private RandomNumberGenerator rng = new();
    private int selectedVariant = 0;

    public override void _Ready()
    {
        if (Textures.Count == 0)
        {
            GD.PrintErr("No textures assigned to MaskDetail: " + DetailName);
            return;
        }

        Texture = Textures[selectedVariant];
        Visible = true;
    }

    public void HideDetail()
    {
        Visible = false;
    }

    public int ShowRandomDetail()
    {
        if (Textures.Count <= 1)
        {
            selectedVariant = 0;
            Texture = Textures[0];
            Visible = true;
            return 0;
        }

        int current = selectedVariant;
        int randomIndex;

        do
        {
            randomIndex = rng.RandiRange(0, Textures.Count - 1);
        } while (randomIndex == current);

        selectedVariant = randomIndex;
        Texture = Textures[randomIndex];
        Visible = true;
        return randomIndex;
    }
}
