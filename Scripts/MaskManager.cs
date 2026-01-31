using Godot;
using System.Collections.Generic;

public partial class MaskManager : Sprite2D
{
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float NoDetailChance = 0.1f;
    public List<MaskDetail> maskDetails = new();

    private RandomNumberGenerator rng = new();
    private int[] referenceMask;

    public override void _Ready()
    {
        maskDetails = GetMaskDetails();
        GD.Print("MaskManager initialized with " + maskDetails.Count + " MaskDetails.");

        HideMask();
        GenerateMask();
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

    public void GenerateReferenceMask()
    {
        referenceMask = GenerateMask();
    }

    private int[] GenerateMask()
    {
        int[] maskConfig = new int[maskDetails.Count];
        rng.Randomize();

        Visible = true;

        for (int i = 0; i < maskDetails.Count; i++)
        {
            if (NoDetailChance > rng.Randf())
            {
                maskDetails[i].HideDetail();
                maskConfig[i] = -1;
            }
            else
            {
                maskConfig[i] = maskDetails[i].ShowRandomDetail();
            }
        }

        GD.Print("Generated Mask Configuration: " + string.Join(", ", maskConfig));
        return maskConfig;
    }

    private void HideMask()
    {
        foreach (var maskDetail in maskDetails)
        {
            maskDetail.HideDetail();
        }
        Visible = false;
    }
}
