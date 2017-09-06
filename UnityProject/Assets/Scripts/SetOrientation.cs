//-----------------------------------------------------------------------
// <copyright file="SetOrientation.cs" company="Google">
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple load function to set the screen orientation
/// </summary>
public class SetOrientation : MonoBehaviour 
{
	/// <summary>
	/// Unity Start function
	/// </summary>
	void Start () 
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;	
	}
	
	/// <summary>
	/// Unity Update function
	/// </summary>
	void Update () 
	{
	}
}
