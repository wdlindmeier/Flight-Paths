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
