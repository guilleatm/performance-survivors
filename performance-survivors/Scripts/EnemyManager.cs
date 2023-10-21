using Godot;
using System;

public partial class EnemyManager : Spawner
{
	[Export] float _enemySpeed;
	[Export] Curve _spawnCurve;
	[Export] int _duration_seconds;
	[Export] int _baseEnemies;

	Node2D _player;

	float _enemiesToSpawn = 0;

	public override void _Ready()
	{
		base._Ready();
		SetCollisionLayerAndMask( (int) CollisionLayer.Second, (int) CollisionLayer.First);
		_player = (Node2D) GetTree().GetNodesInGroup(nameof(Group.Player))[0];
	}


	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		UpdateSpawn(delta);
		UpdateObjectsPosition(delta);
	}

	void UpdateSpawn(double delta)
	{
		float normalizedTime = Time.GetTicksMsec() / 1000f / _duration_seconds;
		float enemyRate = _spawnCurve.Sample(normalizedTime);

		_enemiesToSpawn += (float) (_baseEnemies * enemyRate * delta);

		int enemiesToSpawn_int = (int) _enemiesToSpawn;

		if (enemiesToSpawn_int > 0)
		{
			Spawn(enemiesToSpawn_int);
			_enemiesToSpawn -= enemiesToSpawn_int;
		}
	}

	void UpdateObjectsPosition(double delta)
	{
		Vector2 target = _player.Position;

		Transform2D transform;
		for (int i = 0; i < CanvasItems.Count; i++)
		{
			if (!IsEnabled(i)) continue;
			transform = PhysicsServer2D.AreaGetTransform(Areas[i]);
			transform.Origin += (target - transform.Origin).Normalized() * (float) delta * _enemySpeed;

			RenderingServer.CanvasItemSetTransform(CanvasItems[i], transform);
			PhysicsServer2D.AreaSetTransform(Areas[i], transform);
		}
	}


	protected override Vector2 GetSpawnPosition()
	{
		const int cases = 4;
		Vector2 screenSize = GetViewportRect().Size;
	
		uint i = GD.Randi() % cases;

		uint x = GD.Randi() % (uint) screenSize.X;
		uint y = GD.Randi() % (uint) screenSize.Y;

		switch(i)
		{
			case 0:
				return new Vector2(0, y); // Left
			case 1:
				return new Vector2(x, 0); // Up
			case 2:
				return new Vector2(screenSize.X, y); // Right
			case 3:
				return new Vector2(x, screenSize.Y); // Down
		}
		return Vector2.Zero;
	}

}
