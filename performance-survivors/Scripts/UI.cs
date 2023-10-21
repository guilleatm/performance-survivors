using Godot;
using System;

public partial class UI : Control
{
	[Export] Label fps;
	[Export] Label instanceCount;
	[Export] Spawner[] spawners;

	public override void _Process(double delta)
	{
		fps.Text = Engine.GetFramesPerSecond().ToString();

		int count = 0;
		for (int i = 0; i < spawners.Length; i++)
		{
			count += spawners[i].Count;
		}

		instanceCount.Text = count.ToString();
	}
}
