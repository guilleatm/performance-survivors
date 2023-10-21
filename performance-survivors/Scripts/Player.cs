using Godot;
using System;

public partial class Player : Node2D
{
	[Export] float speed;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			LookAt(mouseMotion.Position);
		}
	}

	string[] inputActions = {"right", "left", "up", "down"};
	public override void _Process(double delta)
	{
		float xAxis = -Input.GetAxis(inputActions[0], inputActions[1]);
		float yAxis = Input.GetAxis(inputActions[2], inputActions[3]);

		Position += new Vector2(xAxis, yAxis) * (float) (delta * speed);
	}
}
