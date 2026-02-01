using Godot;
using System;

public partial class ClipboardCheckBox : HBoxContainer
{
	[Export] public LabelSettings disabledSettings;
	[Export] public LabelSettings enabledSettings;

	public bool Checked { get => button.ButtonPressed; set => button.ButtonPressed = value; }
	public string Text { get => label.Text; set => label.Text = value; }
	public bool Disabled { get => button.Disabled; set => SetDisable(value); }

	private Button button;
	private AnimatedSprite2D checkboxSprite;
	private Label label;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");
		checkboxSprite = GetNode<AnimatedSprite2D>("Button/AnimatedCheckBox");
		button = GetNode<Button>("Button");

		checkboxSprite.PlayBackwards("fill");

		button.Pressed += OnButtonPressed;
	}

	public void SetDisable(bool option)
	{
		button.Disabled = option;
		label.LabelSettings = option ? disabledSettings : enabledSettings;
	}

	public void Mark()
	{
		//GD.Print("Marking checkbox");
		checkboxSprite.Play("fill");
		Checked = true;
	}

	public void Unmark()
	{
		//GD.Print("Unmarking checkbox");
		checkboxSprite.PlayBackwards("fill");
		Checked = false;
	}

	private void OnButtonPressed()
	{
		if (Checked)
		{
			Mark();
		}
		else
		{
			Unmark();
		}
	}
}
