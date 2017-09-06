//-----------------------------------------------------------------------
// <copyright file="SplineReader.cs" company="Google">
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
using System.Collections.Generic;
using SimpleJSON;

/// <summary>
/// Loads up all of the flights / splines for a given airport
/// </summary>
public class SplineReader : MonoBehaviour 
{
	/// <summary>
	/// A dictionary of all loaded flights by key
	/// </summary>
	private Dictionary<string, Flight> m_cachedFlights = new Dictionary<string, Flight>();

	/// <summary>
	/// A sorted list of flights by start time
	/// </summary>
	private List<Flight> m_orderedFlights = new List<Flight>();

	/// <summary>
	/// The amount of time in seconds that the initial animation should last
	/// </summary>
	public double m_animationDuration;

	/// <summary>
	/// The current relative time of the animation
	/// </summary>
    private double m_currentTime = 0.0;

	/// <summary>
	/// The material used to render the splines
	/// </summary>
	public Material m_flightPathMaterial;

	/// <summary>
	/// The abbreviation for the airport
	/// </summary>
    public string m_airportCode;

	/// <summary>
	/// A reference to the dimming sphere which is animated during the transition
	/// </summary>
	public GameObject m_dimmingSphere = null;

	/// <summary>
	/// A hard-coded scale for the airport models
	/// </summary>
	public static Vector3 ModelScale = new Vector3(5.0f, 0.1f, 5.0f);

	/// <summary>
	/// Provides a path to the folder that contains airport spline data
	/// </summary>
	public static string GetDataPath(string airportCode)
	{
		return Application.streamingAssetsPath + "/" + airportCode;
	}

	/// <summary>
	/// Unity Start function
	/// </summary>
	void Start()
    {
		Debug.Log("Loading...");

        LoadFlights();
        
        m_orderedFlights.Clear();
        foreach (KeyValuePair<string, Flight> entry in m_cachedFlights)
        {
            entry.Value.isActive = true;
            m_orderedFlights.Add(entry.Value);
        }
        m_orderedFlights.Sort((x, y) => x.startTime.CompareTo(y.startTime));
        
		// Scale and rotate AFTER the flights have loaded because the line renderers are setup in world space
        transform.localScale = SplineReader.ModelScale;
		transform.localRotation = Quaternion.Euler(0, 180, 0);
        
		gameObject.SetActive(false);

		Reset();
    }

	/// <summary>
	/// Called on Start. Loads the airport's flights from the data folder.
	/// </summary>
	public void LoadFlights()
    {
		string jsonPath = SplineReader.GetDataPath( m_airportCode ) + "/manifest.json";
		Debug.Log("jsonPath: " + jsonPath);
		    
        string jsonText;
        #if  UNITY_EDITOR
            if ( !System.IO.File.Exists(jsonPath) ) 
            {
				Debug.Log("JSON doesn't exist: " + jsonPath);
                return;
            }
            jsonText = System.IO.File.ReadAllText(jsonPath);
        #elif UNITY_ANDROID
            WWW reader = new WWW (jsonPath);
            while (!reader.isDone) {
            }
            jsonText = reader.text;
        #endif

		Debug.Log("Loaded json...");

        var rootNode = JSON.Parse(jsonText);
        int numListed = 0;

        foreach (KeyValuePair<string, JSONNode> entry in rootNode.AsObject)
        {
            numListed += 1;

            var flightInfo = entry.Value;
            Flight flight = null;
            bool hasFlight = m_cachedFlights.TryGetValue(entry.Key, out flight);

            if (!hasFlight)
            {
                string toAirp = flightInfo["to"].Value;
                string fromAirp = flightInfo["from"].Value;
                int startTimestamp = flightInfo["start"].AsInt;
                int endTimestamp = flightInfo["end"].AsInt;

                flight = new Flight(entry.Key, fromAirp, toAirp,
                                    startTimestamp, endTimestamp, 
				                    transform, m_airportCode, m_flightPathMaterial);

                flight.gameObject.layer = this.gameObject.layer;

				if ( !flight.didLoad )
                {
					Debug.Log("Error: Flight didn't load: " + entry.Key);
                }
                m_cachedFlights[entry.Key] = flight;
            }
        }
		Debug.Log("Loaded " + m_cachedFlights.Count + "/" + numListed + " flights");

    }

	/// <summary>
	/// Resets the animation progress.
	/// </summary>
	public void Reset()
	{
		m_currentTime = m_animationDuration * -0.5;
		for ( int i = 0; i < m_orderedFlights.Count; ++i )
		{
			Flight flight = m_orderedFlights[i];
			flight.progress = 0;
			flight.loopingProgress = 0;
		}
		m_dimmingSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.clear);
	}

	/// <summary>
	/// Unity Update function
	/// </summary>
	void Update () 
	{
		
		// Update the flight loading
        foreach ( Flight flight in m_orderedFlights )
        {
            flight.Update();
        }

		m_currentTime += Time.deltaTime;

		if ( m_currentTime > m_animationDuration * 0.5 )
		{
			//Reset();
		}

		float unitTime = Mathf.Min(1.0f, (float)(m_currentTime / (m_animationDuration * 0.5)));

        float fadeSphereTime = Mathf.Min((unitTime+1.0f) / 0.3333f, 1.0f);
        m_dimmingSphere.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f,0.0f,0.0f,0.75f*fadeSphereTime));

		for (int i = 0; i < m_orderedFlights.Count; ++i)
        {
            Flight flight = m_orderedFlights[i];
            float unitFlight = (float)i / m_orderedFlights.Count;
            flight.isActive = true;
			flight.progress = Mathf.Min(unitTime + unitFlight, 1.0f);
			if ( flight.progress >= 1.0f )
			{
				// Make this slower
				float loopTime = Mathf.Repeat((float)(m_currentTime / (m_animationDuration * 0.5)), 2.0f) - 1.0f;
				flight.loopingProgress = Mathf.Min(loopTime + unitFlight, 1.0f);
			}
			else
			{
				flight.loopingProgress = flight.progress;
			}
        }
	}
}
