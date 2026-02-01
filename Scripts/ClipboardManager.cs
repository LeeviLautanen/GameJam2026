using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClipboardManager : Control
{
    [Export] public PackedScene checkBoxScene;

    private MaskManager maskManager;
    private LevelManager levelManager;
    private List<ClipboardCheckBox> checkBoxes;
    private VBoxContainer diffList;

    public override void _Ready()
    {
        maskManager = GetNode<MaskManager>("../Guest/Mask");
        if (maskManager == null)
        {
            GD.PrintErr("DifferenceList didnt find the reference mask");
            return;
        }

        levelManager = GetParent<LevelManager>();
        if (levelManager == null)
        {
            GD.PrintErr("DifferenceList didnt find the LevelManager");
            return;
        }

        levelManager.Connect("GuestProcessed", new Callable(this, nameof(OnGuestProcessed)));

        diffList = GetNode<VBoxContainer>("DifferenceList");

        checkBoxes = new();

        foreach (var maskDetail in maskManager.GetMaskDetails())
        {
            var checkbox = checkBoxScene.Instantiate<ClipboardCheckBox>();
            diffList.AddChild(checkbox);
            checkBoxes.Add(checkbox);
            checkbox.Text = maskDetail.DetailName;
        }

        var submit = GetNode<Button>("Submit");
        submit.Pressed += OnSubmit;
    }

    private void OnGuestProcessed(int guestNumber)
    {
        foreach (var checkbox in checkBoxes)
        {
            checkbox.Disabled = false;
            checkbox.Unmark();
        }
    }

    private void OnSubmit()
    {
        bool[] selectedDetails = new bool[checkBoxes.Count];

        for (int i = 0; i < checkBoxes.Count; i++)
        {
            if (checkBoxes[i].Checked)
            {
                selectedDetails[i] = true;
            }
        }

        bool[] correctAnswers = levelManager.DifferenceSubmitted(selectedDetails);

        if (correctAnswers.Length != checkBoxes.Count)
        {
            return;
        }

        for (int i = 0; i < checkBoxes.Count; i++)
        {
            if (!correctAnswers[i] && selectedDetails[i])
            {
                GD.Print("Some selected answers were incorrect.");
                foreach (var checkbox in checkBoxes)
                {
                    if (checkbox.Checked && !checkbox.Disabled) checkbox.Unmark();
                }
                return;
            }
        }

        for (int i = 0; i < checkBoxes.Count; i++)
        {
            if (correctAnswers[i] && selectedDetails[i])
            {
                checkBoxes[i].Disabled = true;
            }
        }
    }

    public void DisableButtonAtIndex(int index)
    {
        if (index < 0 || index >= checkBoxes.Count)
            return;

        checkBoxes[index].Disabled = true;
    }
}
