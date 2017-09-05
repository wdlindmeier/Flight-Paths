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
