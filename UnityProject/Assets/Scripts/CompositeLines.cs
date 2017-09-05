using UnityEngine;

/// <summary>
/// Composites the spline texture on top of the camera feed.
/// This lets us add some post-processing effects to the splines.
/// </summary>
public class CompositeLines : MonoBehaviour 
{
	/// <summary>
	/// The material / shader used to blit
	/// </summary>
	public Material m_effectMaterial;

	/// <summary>
	/// Blits the spline camera texture onto the main camera texture
	/// </summary>
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		m_effectMaterial.SetTexture("_RenderTexture", source);
		Graphics.Blit(null, destination, m_effectMaterial);
	}
}
