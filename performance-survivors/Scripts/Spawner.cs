using Godot;
using System;
using System.Collections.Generic;

public partial class Spawner : Node2D
{
	[Flags]
	public enum CollisionLayer
	{
		None = 0, // 0000
		First = 1, // 0001
		Second = 2, // 0010
		Third = 4, // 0100
		Forth = 8, // 1000
	}
	[Export] Texture2D _texture;

	public int Count => CanvasItems.Count;
	protected List<Rid> CanvasItems, Areas;
	List<bool> _enableds;
	Stack<int> _disableds, _toDisable;
	Rect2 _textureRect;
	Rid _textureRID, _shape;
	uint _collisionLayer, _collisionMask;

	protected bool IsEnabled(int index) => _enableds[index];


	public override void _Ready()
	{
		CanvasItems = new List<Rid>();
		Areas = new List<Rid>();
		_enableds = new List<bool>();
		_disableds = new Stack<int>();
		_toDisable = new Stack<int>();


		Vector2 textureSize = _texture.GetSize();
		_textureRect = new Rect2(-textureSize.X / 2, -textureSize.Y / 2, textureSize.X, textureSize.Y);
		_textureRID = _texture.GetRid();

		_shape = PhysicsServer2D.CircleShapeCreate();
		Variant radius = textureSize.X / 2f;
		PhysicsServer2D.ShapeSetData(_shape, radius);
	}

	public override void _PhysicsProcess(double delta)
	{
		while(_toDisable.Count > 0)
		{
			_Disable(_toDisable.Pop());
		}
	}

	protected void Spawn(int n = 1)
	{
		for (int i = 0; i < n; i++)
		{

			Transform2D transform = new Transform2D(GetSpawnRotation(), GetSpawnPosition());
			if (_disableds.Count > 0)
			{
				int index = _Enable();

				PhysicsServer2D.AreaSetTransform(Areas[index], transform);
				RenderingServer.CanvasItemSetTransform(CanvasItems[index], transform);

				continue;
			}

			// RENDERING
			Rid canvasItem = RenderingServer.CanvasItemCreate();

			RenderingServer.CanvasItemSetParent(canvasItem, GetCanvas());
			RenderingServer.CanvasItemAddTextureRect(canvasItem, _textureRect, _textureRID);

			RenderingServer.CanvasItemSetTransform(canvasItem, transform);

			CanvasItems.Add(canvasItem);

			// PHYSICS
			Rid area = PhysicsServer2D.AreaCreate();

			PhysicsServer2D.AreaAddShape(area, _shape);
			PhysicsServer2D.AreaSetTransform(area, transform);
			PhysicsServer2D.AreaSetCollisionLayer(area, _collisionLayer);
			PhysicsServer2D.AreaSetCollisionMask(area, _collisionMask);

			PhysicsServer2D.AreaSetSpace(area, GetWorld2D().Space);

			PhysicsServer2D.AreaSetMonitorable(area, true);

			int IX = Areas.Count;
			Callable onAreaOverlapp = Callable.From( 
				(int status, Rid otherAreaRID, int otherInstanceObjectID, int otherAreaShapeIX, int areaShapeIX) => 
				{
					OnAreaOverlapp(IX, status, otherAreaRID, otherInstanceObjectID, otherAreaShapeIX, areaShapeIX);
				});

			PhysicsServer2D.AreaSetAreaMonitorCallback(area, onAreaOverlapp);

			Areas.Add(area);
			_enableds.Add(true);
		}
	}

	protected void Disable(int index)
	{
		_toDisable.Push(index);
	}
	void _Disable(int index)
	{
		if (!_enableds[index]) return;
		PhysicsServer2D.AreaSetShapeDisabled(Areas[index], 0, true);
		//PhysicsServer2D.AreaSetMonitorable(areas[index], false);
		RenderingServer.CanvasItemSetVisible(CanvasItems[index], false);

		_disableds.Push(index);
		_enableds[index] = false;
	}

	int _Enable()
	{
		int index = _disableds.Pop();
		PhysicsServer2D.AreaSetShapeDisabled(Areas[index], 0, false);
		//PhysicsServer2D.AreaSetMonitorable(areas[index], true);
		RenderingServer.CanvasItemSetVisible(CanvasItems[index], true);
		_enableds[index] = true;

		return index;
	}

	void OnAreaOverlapp(int index, int status, Rid otherAreaRID, int otherInstanceObjectID, int otherAreaShapeIX, int areaShapeIX)
	{
		if (status == (int) PhysicsServer2D.AreaBodyStatus.Added)
		{
			Disable(index);
		}
	}

	/// <summary>
	/// Sets collision layer and mask for all spawned objects.
	/// </summary>
	/// <param name="_collisionLayer">The layer where the object is.</param>
	/// <param name="_collisionMask">The layer where the object checks collisions.</param>
	protected void SetCollisionLayerAndMask(int _collisionLayer, int _collisionMask)
	{
		this._collisionLayer = (uint) _collisionLayer;
		this._collisionMask = (uint) _collisionMask;
	}

	protected virtual Vector2 GetSpawnPosition()
	{
		return Vector2.Zero;
	}

	protected virtual float GetSpawnRotation()
	{
		return 0;
	}

	public override void _ExitTree()
	{
		for (int i = 0; i < CanvasItems.Count; i++)
		{
			RenderingServer.FreeRid(CanvasItems[i]);
			PhysicsServer2D.FreeRid(Areas[i]);
		}
	}
}
