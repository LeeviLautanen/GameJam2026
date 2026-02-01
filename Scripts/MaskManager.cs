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
    private Node2D guestScene;

    public override void _Ready()
    {
        guestScene = GetNode<Node2D>("../GuestScene");
        if (guestScene == null)
        {
            GD.PrintErr("Guest not found in MaskManager.");
            return;
        }

        rng.Randomize();
        maskDetails = GetMaskDetails();
        MaskData = new int[maskDetails.Count];
        GD.Print("MaskManager initialized with " + maskDetails.Count + " MaskDetails.");
    }

    public void HideGuestSprite()
    {
        guestScene.Visible = false;
    }

    public void ShowGuestSprite()
    {
        guestScene.Visible = true;
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
        float RemoveChance = 0.2f;
        float ChangeChance = 0.9f;
        int[] newData = new int[maskDetails.Count];

        for (int i = 0; i < maskDetails.Count; i++)
        {
            float r = rng.Randf();
            if (r < RemoveChance)
            {
                maskDetails[i].HideDetail();
                newData[i] = -1;
            }
            else if (r < RemoveChance + ChangeChance)
            {
                newData[i] = maskDetails[i].ShowRandomDetail();
            }
            else
            {
                newData[i] = MaskData[i];
            }
        }

        GD.Print("Generated reference mask: " + string.Join(", ", newData));

        MaskData = newData;
    }

    public void GenerateMask(int[] referenceData)
    {
        int[] newData = new int[maskDetails.Count];

        if (MaskData == null || maskDetails.Count != referenceData.Length)
        {
            GD.PrintErr("MaskData array is not properly initialized.");
            return;
        }

        if (referenceData != null)
        {
            GD.Print("Reference Mask Data: " + string.Join(", ", maskDetails));

            for (int i = 0; i < maskDetails.Count; i++)
            {
                MaskDetail detail = maskDetails[i];
                detail.SetDetail(referenceData[i]);
                newData[i] = referenceData[i];
            }
        }

        for (int i = 0; i < maskDetails.Count; i++)
        {
            float r = rng.Randf();
            if (r < RemoveDetailChance)
            {
                maskDetails[i].HideDetail();
                newData[i] = -1;
            }
            else if (r < RemoveDetailChance + ChangeDetailChance)
            {
                newData[i] = maskDetails[i].ShowRandomDetail();
            }
        }

        GD.Print("Generated Mask: " + string.Join(", ", newData));

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
