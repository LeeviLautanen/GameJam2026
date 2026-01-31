using Godot;
using System.Collections.Generic;

public partial class MaskManager : Sprite2D
{
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float RemoveDetailChance = 0.1f;
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float ChangeDetailChance = 0.3f;
    public List<MaskDetail> maskDetails = new();
    public int[] MaskData;

    private RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        maskDetails = GetMaskDetails();
        MaskData = new int[maskDetails.Count];
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
            float r = rng.Randf();
            if (r < RemoveDetailChance)
            {
                GD.Print("Hiding detail: " + maskDetails[i].DetailName);
                maskDetails[i].HideDetail();
                newData[i] = -1;
            }
            else if (r < RemoveDetailChance + ChangeDetailChance)
            {
                GD.Print("Changing detail: " + maskDetails[i].DetailName);
                newData[i] = maskDetails[i].ShowRandomDetail();
            }
            else
            {
                newData[i] = MaskData[i];
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
