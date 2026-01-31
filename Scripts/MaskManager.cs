using Godot;
using System.Collections.Generic;

public partial class MaskManager : Sprite2D
{
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

        for (int i = 0; i < maskDetails.Count; i++)
        {
            if (rng.RandiRange(0, 1) == 0)
            {
                maskDetails[i].HideDetail();
                maskConfig[i] = -1;
            }
            else
            {
                maskConfig[i] = maskDetails[i].ShowRandomDetail();
            }
        }

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
