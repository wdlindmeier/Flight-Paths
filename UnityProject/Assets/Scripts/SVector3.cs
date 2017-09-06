//-----------------------------------------------------------------------
// <copyright file="SVector3.cs" company="Google">
//
// Copyright 2017 Google Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     https://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
