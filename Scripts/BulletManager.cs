using Godot;
using System;

public partial class BulletManager : Spawner
{
	[Export] float bulletSpeed;
	[Export] float bulletSpawnRate;

	float bulletsToSpawn = 0;
	bool shooting = false;
	
	public override void _Ready()
	{
		base._Ready();
		SetCollisionLayerAndMask( (int) CollisionLayer.First, (int) CollisionLayer.Second);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			shooting = mouseButton.Pressed;
		}
	}

	// public override void _Process(double delta)
	// {

	// }

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		UpdateSpawn(delta);
		UpdateObjectsPosition(delta);
	}

	void UpdateSpawn(double delta)
	{
		if (!shooting) return;
		bulletsToSpawn += (float) (bulletSpawnRate * delta);

		int bulletsToSpawn_int = (int) bulletsToSpawn;

		if (bulletsToSpawn_int > 0)
		{
			Spawn(bulletsToSpawn_int);
			bulletsToSpawn -= bulletsToSpawn_int;
		}
	}

	void UpdateObjectsPosition(double delta)
	{
		float radius = GetViewportRect().Size.Length();

		Transform2D transform;
		for (int i = 0; i < CanvasItems.Count; i++)
		{
			if (!IsEnabled(i)) continue;
			transform = PhysicsServer2D.AreaGetTransform(Areas[i]);

			if (transform.Origin.Length() > radius)
			{
				Disable(i);
				continue;
			}

			transform.Origin += Vector2.Right.Rotated(transform.Rotation) * (float) (delta * bulletSpeed);

			PhysicsServer2D.AreaSetTransform(Areas[i], transform);
			RenderingServer.CanvasItemSetTransform(CanvasItems[i], transform);
		}
	}

	protected override Vector2 GetSpawnPosition()
	{
		return GlobalPosition;
	}
	protected override float GetSpawnRotation()
	{
		return GlobalRotation;
	}
}
