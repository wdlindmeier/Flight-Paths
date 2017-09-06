//-----------------------------------------------------------------------
// <copyright file="TrackCamera.cs" company="Google">
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

/// <summary>
/// Keeps a camera tracked to another Camera
/// </summary>
public class TrackCamera : MonoBehaviour 
{
	public Camera m_camera;

	/// <summary>
	/// Unity start function
	/// </summary>
	void Start () 
	{
	}

	/// <summary>
	/// Unity Update function
	/// </summary>
	void Update () 
	{
		transform.position = m_camera.transform.position;
		transform.rotation = m_camera.transform.rotation;
		GetComponent<Camera>().aspect = m_camera.aspect;
		GetComponent<Camera>().fieldOfView = m_camera.fieldOfView;
		GetComponent<Camera>().farClipPlane = m_camera.farClipPlane;
		GetComponent<Camera>().nearClipPlane = m_camera.nearClipPlane;
	}
}
