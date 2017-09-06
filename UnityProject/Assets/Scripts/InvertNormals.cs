//-----------------------------------------------------------------------
// <copyright file="InvertNormals.cs" company="Google">
//
// Copyright 2017 Google Inc.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.
//
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper behavior to invert a mesh normals
/// </summary>
public class InvertNormals : MonoBehaviour 
{
	/// <summary>
	/// Unity Start function
	/// </summary>
	void Start () 
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
 		mesh.triangles = mesh.triangles.Reverse().ToArray();
	}
	
	/// <summary>
	/// Unity Update function
	/// </summary>
	void Update () 
	{
	}
}
