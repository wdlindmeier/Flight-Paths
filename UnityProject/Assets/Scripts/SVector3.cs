//-----------------------------------------------------------------------
// <copyright file="SVector3.cs" company="Google">
//
// Copyright (c) 2017 Google Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// </copyright>
//-----------------------------------------------------------------------

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
