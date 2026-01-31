using Godot;
using System;

public partial class DifferenceList : Control
{
    private MaskManager maskManager;
    private ButtonGroup group = new ButtonGroup();
    private VBoxContainer diffList;

    public override void _Ready()
    {
        maskManager = (MaskManager)FindNodeWithScript<MaskManager>(GetTree().Root);
        if (maskManager == null)
        {
            GD.PrintErr("MaskManager not found in the scene tree.");
            return;
        }

        diffList = GetNode<VBoxContainer>("DifferenceList");

        // Create a single ButtonGroup resource
        var group = new ButtonGroup();

        foreach (var maskDetail in maskManager.GetMaskDetails())
        {
            var radio = new CheckBox();
            radio.Text = maskDetail.DetailName;
            radio.ToggleMode = true;
            radio.ButtonGroup = group;

            diffList.AddChild(radio);
        }

        var submit = GetNode<Button>("Submit");
        //submit.Pressed += OnSubmitPressed;
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
