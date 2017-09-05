using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using UnityEngine.EventSystems;

/// <summary>
/// Controlls the Flight Path example.
/// </summary>
public class NavigationController : MonoBehaviour
{
    /// <summary>
    /// The first-person camera being used to render the AR view.
    /// </summary>
    public Camera m_firstPersonCamera;

    /// <summary>
    /// The current airport model
    /// </summary>
    public GameObject m_selectedAirspace = null;

	/// <summary>
	/// A translucent sphere that dims around the FP camera when the airspace is visible
	/// </summary>
	public GameObject m_skyboxDim;

	/// <summary>
	/// A texture quad that indicates where the user will place the airport
	/// </summary>
	public GameObject m_placementShadow;

	/// <summary>
	/// A UI image that tells the user that no planes have been found
	/// </summary>
	public Image m_lookingImage;

	/// <summary>
	/// The position to lerp the placement target to
	/// </summary>
	private Vector3 m_placementTargetPos;

    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_skyboxDim.SetActive(false);
		Vector3 ringScale = m_placementShadow.transform.localScale;
		m_placementShadow.transform.localScale = new Vector3(ringScale.x * SplineReader.ModelScale.x,
														     ringScale.y * SplineReader.ModelScale.z,
														     ringScale.z);
    }

	/// <summary>
    /// The Unity Update() method.
    /// </summary>
    void Update()
    {
		bool hasTouch = Input.touchCount > 0;
		if ( hasTouch )
		{
			// Ignore touches on buttons
			bool isTouch0OverButton = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
			hasTouch = !isTouch0OverButton;
		}

		bool didFindSurface = false;

		if ( m_selectedAirspace == null )
		{
			m_placementShadow.SetActive(false);
		}
		else 
		{
			if ( !hasTouch && !m_selectedAirspace.activeInHierarchy )
			{					
				TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds |
												 TrackableHitFlag.PlaneWithinPolygon;
				TrackableHit hit;
				Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
				didFindSurface = Session.Raycast(m_firstPersonCamera.ScreenPointToRay(center), raycastFilter, out hit);

				if ( didFindSurface )
				{
					m_placementTargetPos = hit.Point;
					if (!m_placementShadow.activeInHierarchy)
					{
						m_placementShadow.SetActive(true);
						m_placementShadow.transform.position = m_placementTargetPos;
					}
				}
			}
			else
			{
				// Hide when touch is down
				m_placementShadow.SetActive(false);
			}
		}

		if ( hasTouch && m_selectedAirspace != null )
        {
			Touch touch = Input.GetTouch(0);
			if ( touch.phase == TouchPhase.Began )
			{
				if (m_selectedAirspace.activeInHierarchy)
				{
					m_selectedAirspace.SetActive(false);
				}
				else
				{
					// Only place where we've presented a surface
					m_selectedAirspace.transform.position = m_placementShadow.transform.position; 
					m_selectedAirspace.GetComponent<SplineReader>().Reset();
					m_selectedAirspace.SetActive(true);
					m_placementShadow.SetActive(false);
				}
			}
        }

		bool areSplinesVisible = m_selectedAirspace != null && m_selectedAirspace.activeInHierarchy;
		#if UNITY_EDITOR
		areSplinesVisible = true;
		#endif
        m_skyboxDim.SetActive(areSplinesVisible);
		if ( areSplinesVisible )
		{
			RenderSettings.fog = true;
			m_lookingImage.gameObject.SetActive(false);
		}
		else
		{
			RenderSettings.fog = false;
			List<TrackedPlane> planes = new List<TrackedPlane>();
			Frame.GetAllPlanes(ref planes);
			if ( planes.Count > 0 )
			{
				m_lookingImage.gameObject.SetActive(false);
			}
			else
			{
				m_lookingImage.gameObject.SetActive(true);
			}
		}
    }

	/// <summary>
	/// The Unity LateUpdate() method.
	/// </summary>
	void LateUpdate()
	{
		Vector3 placementPos = m_placementShadow.transform.position;
		m_placementShadow.transform.position = Vector3.Lerp(placementPos, m_placementTargetPos, 0.25f);
	}
}