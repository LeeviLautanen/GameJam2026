using Godot;
using System;
using System.Linq;

public partial class DifferenceList : Control
{
    private MaskManager maskManager;
    private LevelManager levelManager;
    private ButtonGroup group;
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

        // Create a single ButtonGroup resource
        group = new ButtonGroup();

        foreach (var maskDetail in maskManager.GetMaskDetails())
        {
            var radio = new CheckBox();
            radio.Text = maskDetail.DetailName;
            radio.ToggleMode = true;
            radio.ButtonGroup = group;

            diffList.AddChild(radio);
        }

        var submit = GetNode<Button>("Submit");
        submit.Pressed += OnSubmit;
    }

    private void OnGuestProcessed(int guestNumber)
    {
        var buttons = group.GetButtons();
        foreach (var button in buttons)
        {
            button.Disabled = false;
        }
    }

    private void OnSubmit()
    {
        var buttons = group.GetButtons();
        int detailIndex = -1;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == group.GetPressedButton())
            {
                detailIndex = i;
                break;
            }
        }

        bool correct = levelManager.DifferenceSubmitted(detailIndex);

        if (correct)
        {
            DisableButtonAtIndex(detailIndex);
        }
    }

    public void DisableButtonAtIndex(int index)
    {
        var buttons = group.GetButtons();
        if (index < 0 || index >= buttons.Count) return;

        buttons[index].Disabled = true;
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
