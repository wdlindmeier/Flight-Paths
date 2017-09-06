//-----------------------------------------------------------------------
// <copyright file="AirportPickerController.cs" company="Google">
//
// Copyright 2017 Google Inc.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.
//
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A controller for picking the selected airport
/// </summary>
public class AirportPickerController : MonoBehaviour 
{
	/// <summary>
	/// The GameObject with the SFO spline reader attatched.
	/// </summary>
	public GameObject m_airspaceSFO;

	/// <summary>
	/// The GameObject with the JFK spline reader attatched.
	/// </summary>
	public GameObject m_airspaceJFK;

	/// <summary>
	/// The canvas with the airport picker UI
	/// </summary>
	public Canvas m_pickerUI;

	/// <summary>
	/// The canvas displayed over the airport with a back button
	/// </summary>
	public Canvas m_worldUI;

	/// <summary>
	/// The back button displayed in world view
	/// </summary>
	public Button m_buttonBack;

	/// <summary>
	/// The sprite indicating that the selected airport is JFK
	/// </summary>
	public Sprite m_spriteBackJFK;

	/// <summary>
	/// The sprite indicating that the selected airport is SFO
	/// </summary>
	public Sprite m_spriteBackSFO;

	/// <summary>
	/// The Unity Start() method.
	/// </summary>
	void Start () 
	{
		m_worldUI.gameObject.SetActive(false);
	}
	
	/// <summary>
    /// The Unity Update() method.
    /// </summary>
	void Update () 
	{
	}

	/// <summary>
	/// The airport button action when in world space
	/// </summary>
	public void LeaveWorldPressed()
	{
		Debug.Log("Leave World");
		m_pickerUI.gameObject.SetActive(true);
		m_worldUI.gameObject.SetActive(false);
		GetComponent<NavigationController>().m_placementShadow.SetActive(false);
		GetComponent<NavigationController>().m_selectedAirspace.SetActive(false);
		GetComponent<NavigationController>().m_selectedAirspace = null;
	}

	/// <summary>
	/// The button action when selecting an airport from the launch screen
	/// </summary>
	public void PickedAirport(string airportName)
	{
		Debug.Log("Airport was picked: " + airportName);

		if (airportName == "JFK")
		{
			GetComponent<NavigationController>().m_selectedAirspace = m_airspaceJFK;
			m_buttonBack.GetComponent<Image>().sprite = m_spriteBackJFK;
		}
		else if (airportName == "SFO")
		{
			GetComponent<NavigationController>().m_selectedAirspace = m_airspaceSFO;
			m_buttonBack.GetComponent<Image>().sprite = m_spriteBackSFO;
		}

		// Disable the airport picker
		m_pickerUI.gameObject.SetActive(false);
		m_worldUI.gameObject.SetActive(true);

		GetComponent<NavigationController>().m_selectedAirspace.GetComponent<SplineReader>().Reset();

		#if UNITY_EDITOR
			// Start animating
			Debug.Log("Enabling airport in editor " + GetComponent<NavigationController>().m_selectedAirspace);
			GetComponent<NavigationController>().m_selectedAirspace.SetActive(true);
		#elif UNITY_ANDROID
    	    // Hide it until the user places it on a plane
		#endif
	}
}
