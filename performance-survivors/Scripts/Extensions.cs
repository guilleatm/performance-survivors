using Godot;
using System;

public static partial class Extensions
{

	public static T GetNode<T>(this Node n) where T : Node
	{
		return n.GetNode<T>(typeof(T).Name);
	}

}
