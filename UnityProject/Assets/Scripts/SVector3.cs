using UnityEngine;
using System;

/// <summary>
/// A serializable Vector3
/// </summary>
[Serializable()]
public class SVector3
{
	/// <summary>
	/// The x component
	/// </summary>
	public float x { get; set; }

	/// <summary>
	/// The y component
	/// </summary>
	public float y { get; set; }

	/// <summary>
	/// The z component
	/// </summary>
	public float z { get; set; }

	/// <summary>
	/// Constructor
	/// </summary>
	public SVector3(float X, float Y, float Z)
	{
		x = X;
		y = Y;
		z = Z;
	}

	/// <summary>
	/// Implicit operator
	/// </summary>
	static public implicit operator SVector3(Vector3 value) 
	{
		return new SVector3(value.x,value.y,value.z);
	}

	/// <summary>
	/// Explicit operator
	/// </summary>
	static public explicit operator Vector3(SVector3 value)
	{
		return new Vector3(value.x,value.y,value.z);
	}
}
