using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DifferenceList : Control
{
    private MaskManager maskManager;
    private LevelManager levelManager;
    private List<CheckBox> checkBoxes;
    private VBoxContainer diffList;

    public override void _Ready()
    {
        maskManager = (MaskManager)FindNodeWithScript<MaskManager>(GetTree().Root);
        if (maskManager == null)
        {
            GD.PrintErr("MaskManager not found in the scene tree.");
            return;
        }

        levelManager = GetParent<LevelManager>();
        if (levelManager == null)
        {
            GD.PrintErr("MaskManager not found in the scene tree.");
            return;
        }

        levelManager.Connect("GuestProcessed", new Callable(this, nameof(OnGuestProcessed)));

        diffList = GetNode<VBoxContainer>("DifferenceList");

        checkBoxes = new();

        foreach (var maskDetail in maskManager.GetMaskDetails())
        {
            var checkbox = new CheckBox();
            checkbox.Text = maskDetail.DetailName;
            checkbox.ToggleMode = true;
            diffList.AddChild(checkbox);
            checkBoxes.Add(checkbox);
        }

        var submit = GetNode<Button>("Submit");
        submit.Pressed += OnSubmit;
    }

    private void OnGuestProcessed(int guestNumber)
    {
        foreach (var checkbox in checkBoxes)
        {
            checkbox.Disabled = false;      // re‑enable
            checkbox.ButtonPressed = false; // uncheck
        }
    }

    private void OnSubmit()
    {
        int detailIndex = -1;

        for (int i = 0; i < checkBoxes.Count; i++)
        {
            if (checkBoxes[i].ButtonPressed)
            {
                detailIndex = i;
                break;
            }
        }

        bool correct = levelManager.DifferenceSubmitted(detailIndex);

        if (correct && detailIndex != -1)
        {
            DisableButtonAtIndex(detailIndex);
        }
    }

    public void DisableButtonAtIndex(int index)
    {
        if (index < 0 || index >= checkBoxes.Count)
            return;

        var checkbox = checkBoxes[index];

        checkbox.Disabled = true;

        checkbox.SetPressedNoSignal(false);
    }

    Node FindNodeWithScript<T>(Node root) where T : Node
    {
        if (root is T) return root;
        foreach (Node child in root.GetChildren())
        {
            var result = FindNodeWithScript<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}
