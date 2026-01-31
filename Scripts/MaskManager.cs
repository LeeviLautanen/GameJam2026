using Godot;
using System.Collections.Generic;

public partial class MaskManager : Sprite2D
{

    public float NoDetailChance = 0.1f;
    public List<MaskDetail> maskDetails = new();
    public int[] MaskData;

    private RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        maskDetails = GetMaskDetails();
        GD.Print("MaskManager initialized with " + maskDetails.Count + " MaskDetails.");
    }

    public List<MaskDetail> GetMaskDetails()
    {
        var list = new List<MaskDetail>();
        foreach (Node child in GetChildren())
        {
            if (child is MaskDetail maskDetail)
                list.Add(maskDetail);
        }
        return list;
    }

    public void GenerateMask()
    {
        int[] newData = new int[maskDetails.Count];
        rng.Randomize();

        for (int i = 0; i < maskDetails.Count; i++)
        {
            if (NoDetailChance > rng.Randf())
            {
                maskDetails[i].HideDetail();
                newData[i] = -1;
            }
            else
            {
                newData[i] = maskDetails[i].ShowRandomDetail();
            }
        }

        MaskData = newData;
    }

    public void HideMask()
    {
        Visible = false;
    }

    public void ShowMask()
    {
        Visible = true;
    }
}
