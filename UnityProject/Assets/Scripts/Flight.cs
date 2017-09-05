using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Flight
{
	/// <summary>
	/// Indicates if a flight is deparing from or arriving to the selected airport.
	/// </summary>
	public enum SelectedAirportRelationship
    {
        Unknown = 0,
        Departed = 1,
        Arrived = 2
    };

	public SelectedAirportRelationship selectedAirportRelationship = SelectedAirportRelationship.Unknown;

	/// <summary>
	/// The 3d positions of the flight path
	/// </summary>
	private Vector3[] m_splinePositions;

	/// <summary>
	/// A reference to the game object
	/// </summary>
	private GameObject m_gameObject;
	public GameObject gameObject { get{ return m_gameObject; } }

	/// <summary>
	/// A reference to the parent transform
	/// </summary>
	private Transform m_parentTransform;

	/// <summary>
	/// The line renderer which draws the path.
	/// </summary>
	private LineRenderer m_line;

	/// <summary>
	/// The abbreviation for the airport from which the flight departed
	/// </summary>
	private string m_fromAbbr;

	/// <summary>
	/// The abbreviation for the airport from which the flight is arriving
	/// </summary>
    private string m_toAbbr;

	/// <summary>
	/// A timestamp indicating the flights start
	/// </summary>
	private long m_startTime = 0; 
	public long startTime { get { return m_startTime; } }

	/// <summary>
	/// A timestamp indicating the flights end
	/// </summary>
	private long m_endTime = 0;
	public long endTime { get { return m_endTime; } }

	/// <summary>
	/// The number of verts in the flight path line
	/// </summary>
	private int m_vertexCount = 0;

	/// <summary>
	/// A proxy for the game object is active
	/// </summary>
	private bool m_isActive = false;

	/// <summary>
	/// The color of the flight path
	/// </summary>
	private Color m_lineColor;

	/// <summary>
	/// A reference to the selected airport code
	/// </summary>
	private string m_selectedAirportCode;

	/// <summary>
	/// The material to use for the flight path
	/// </summary>
	private Material m_materialLine;

	private bool _didLoad = false;
	public bool didLoad { get { return _didLoad; } }

	/// <summary>
	/// The amount of the flight which should be rendered based on the passage of time
	/// </summary>
	private float m_progress = 0.0f;
	public float progress
	{
		get { return m_progress; }
		set { m_progress = value; }
	}

	/// <summary>
	/// The looping "playhead" of the flight animation
	/// </summary>
	private float m_loopingProgress = 0.0f;
	public float loopingProgress
	{
		get { return m_loopingProgress; }
		set
		{
			if (m_loopingProgress != value)
			{
				m_loopingProgress = value;
				SetActiveLineProgress();
			}
		}
	}

	/// <summary>
	/// The unique ID for this flight which maps to the data file
	/// </summary>
	private string m_key = "";

	/// <summary>
	/// Constructor
	/// </summary>
	public Flight(string key, string fromAirp, string toAirp, long startTime,
				   long endTime, Transform parent, string selectedAirportCode,
				   Material flightPathMaterial)
	{
		m_progress = 0;
		m_selectedAirportCode = selectedAirportCode;
		m_fromAbbr = fromAirp;
		m_toAbbr = toAirp;
		m_materialLine = new Material(flightPathMaterial);
		bool startsInSelectedAirport = fromAirp.CompareTo(m_selectedAirportCode) == 0;

		selectedAirportRelationship = startsInSelectedAirport ?
				Flight.SelectedAirportRelationship.Departed :
				Flight.SelectedAirportRelationship.Arrived;

		m_key = key;
		m_startTime = startTime;
		m_endTime = endTime;
		m_parentTransform = parent;

		LoadSpline();
	}

	/// <summary>
	/// A proxy for the GameObject isActive
	/// </summary>
	public bool isActive 
	{ 
		get { return m_isActive; }
		set 
		{
			if ( m_isActive != value )
			{
				m_isActive = value;
				if (m_gameObject)  m_gameObject.SetActive(m_isActive);
			}
		}
	}

	/// <summary>
	/// Updates the line renderer with the current progress and looping progress
	/// </summary>
	private void SetActiveLineProgress()
	{
        if ( m_isActive && m_line )
		{
			// A simple 2 color gradient with a fixed alpha of 1.0f.

			float gradProgress = m_progress;
			float flightPosProgress = m_loopingProgress;
			float alphaA = 0.0f;
			float alphaB = 0.0f;

			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] 
			{ 				
				new GradientAlphaKey(1, 0.0f),
				new GradientAlphaKey(1, 1.0f) 
			};
		
			if ( m_progress > 0 )
			{
				alphaA = 1.0f;
				alphaB = 0.0f;
				if ( selectedAirportRelationship == SelectedAirportRelationship.Departed )
				{
					gradProgress = 1.0f-m_progress;					
					flightPosProgress = 1.0f-m_loopingProgress;
					alphaA = 0.0f;
					alphaB = 1.0f;
				}				
			}

			float timeA = Mathf.Max(0.0f, gradProgress*0.99f);
			float timeB = Mathf.Min(1.0f, gradProgress*1.01f);

			if ( selectedAirportRelationship == SelectedAirportRelationship.Departed )
			{
				// Blue
				alphaKeys = new GradientAlphaKey[] 
				{ 				
					new GradientAlphaKey(0, 0.0f),  // Fade out on the edges
					new GradientAlphaKey(alphaA, Mathf.Max(timeA, 0.1f)), 
					new GradientAlphaKey(alphaA, Mathf.Max(timeA, 0.1f)), 
					new GradientAlphaKey(alphaB, Mathf.Max(timeB, 0.12f)),
					new GradientAlphaKey(alphaB, 1.0f) 
				};
			}
			else 
			{
				// Pink
				alphaKeys = new GradientAlphaKey[] 
				{ 				
					new GradientAlphaKey(0, 0.0f),  // Fade out on the edges
					new GradientAlphaKey(alphaA, Mathf.Min(timeA, 0.1f)), 
					new GradientAlphaKey(alphaA, timeA), 
					new GradientAlphaKey(alphaB, timeB),
					new GradientAlphaKey(alphaB, 1.0f) 
				};
			}

			Gradient gradient = new Gradient();

			float flightTimeA = Mathf.Max(0.0f, flightPosProgress * 0.995f);
			float flightTimeB = Mathf.Min(1.0f, flightPosProgress * 1.005f);

			// The flight color be at least 1 segment
			float flightLength = Mathf.Abs(flightTimeA - flightTimeB);
			float segmentLength = 1.0f / (m_line.positionCount / 2.0f);
			if ( flightLength < segmentLength )
			{
				float margin = (segmentLength - flightLength) / 2.0f;
				flightTimeA -= margin * 1.1f;
				flightTimeB += margin * 1.1f;
			}

			gradient.SetKeys(
			new GradientColorKey[] { 
				new GradientColorKey(m_lineColor, 0.0f), 
				new GradientColorKey(m_lineColor, flightTimeA-0.001f),
				new GradientColorKey(Color.white, flightTimeA),
				new GradientColorKey(Color.white, flightTimeB),
				new GradientColorKey(m_lineColor, flightTimeB+0.001f),
				new GradientColorKey(m_lineColor, 1.0f) 
				},
				alphaKeys
			);

			m_line.colorGradient = gradient;
        }
    }

	/// <summary>
	/// Load the spline. This is split into 2 functions to hypothetically load it on
	/// a background thread. Ignoring this optimization for now, since there are only
	/// 2 airports.
	/// </summary>
	private bool LoadSpline()
	{
		_didLoad = LoadData();
		if ( _didLoad )
		{
			LoadDataCompleted();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Read spline data from binary format on disk
	/// </summary>
	private bool LoadData() 
	{
        string pointsPath = SplineReader.GetDataPath(m_selectedAirportCode) + "/" + m_key + ".bin";

		List<SVector3> sPositions = null;

#if  UNITY_EDITOR

		if ( !File.Exists(pointsPath) )
		{
			Debug.Log("ERROR: Spline path doesnt exist: " + pointsPath);
			return false;
		}
		try
		{			
			using ( Stream stream = File.Open(pointsPath, FileMode.Open) )
			{
				BinaryFormatter bin = new BinaryFormatter();
				sPositions = (List<SVector3>)bin.Deserialize(stream);
			}
        }
		catch (Exception exc)
		{
			Debug.Log("Exception: " + exc.Message);
			return false;
		}

#elif UNITY_ANDROID

		try
		{			
	        WWW reader = new WWW (pointsPath);
	        while (!reader.isDone) {
	        }
	        MemoryStream stream = new MemoryStream(reader.bytes);
	        BinaryFormatter bin = new BinaryFormatter();
			sPositions = (List<SVector3>)bin.Deserialize(stream);
			stream.Close();
		}
		catch (Exception exc)
		{
			Debug.Log("Exception: " + exc.Message);
			return false;
		}

#endif

		m_vertexCount = sPositions.Count;
		m_splinePositions = new Vector3[m_vertexCount];

        for ( int j = 0; j < m_vertexCount; ++j )
		{
			int idx = j;

			m_splinePositions[j] = (Vector3)sPositions[idx];
		}

		return true;
	}

	/// <summary>
	/// Setup the game object once the spline data has been loaded.
	/// </summary>
	private void LoadDataCompleted()
	{
		// After load
		if ( m_line )
		{			
			throw new System.Exception("m_line already exists.");
		}

		if ( m_gameObject )
		{
			throw new System.Exception("m_gameObject already exists.");
		}

        Quaternion initialRotation = m_parentTransform.localRotation;
        Vector3 initialScale = m_parentTransform.lossyScale;// localScale;
        Vector3 initialPosition = m_parentTransform.localPosition;
        m_gameObject = new GameObject ();
        m_gameObject.transform.parent = m_parentTransform;
        // NOTE: The LineRenderer is not smart about the initial transform, so we have to bake that into the game object
		m_gameObject.transform.localRotation = Quaternion.Inverse(initialRotation);// * Quaternion.Euler(0,10,0); // NOTE: The globe has a known offset of 10 deg
        m_gameObject.transform.localScale = new Vector3(1, 1, 1);// initialScale;
        m_gameObject.transform.localPosition = initialPosition;
        m_gameObject.SetActive(m_isActive);

		if ( m_splinePositions == null || m_splinePositions.Length == 0 )
		{
			return;
		}

        m_line = m_gameObject.AddComponent<LineRenderer>();

		m_line.startWidth = 0.00035f * SplineReader.ModelScale.x;
		m_line.endWidth = 0.00035f * SplineReader.ModelScale.x;
		m_line.startColor = new Color(0,0,0,0);
		m_line.endColor = new Color(0,0,0,0);

		if ( selectedAirportRelationship == SelectedAirportRelationship.Departed )
		{

			m_lineColor = new Color(0, 1.0f, 1.0f, 0.0f);
		}
		else if ( selectedAirportRelationship == SelectedAirportRelationship.Arrived )
		{

			m_lineColor = new Color(1.0f, 0, 1.0f, 0.0f);
		}
		else 
		{
			m_lineColor = new Color(0, 1.0f, 0, 0.0f);
		}

		m_line.positionCount = m_vertexCount;
		m_line.SetPositions(m_splinePositions);

		m_line.material = m_materialLine;
		m_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		m_line.useWorldSpace = false;

		SetActiveLineProgress();
    }

	/// <summary>
	/// Unity update function
	/// </summary>
	public void Update()
    {
    }
}