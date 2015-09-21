using UnityEngine;
using System.Collections;

// Add this components to objects that are a supporting surface
// Can (and should in some cases) attach to subobjects

public class SupportingSurface : MonoBehaviour {

	public enum SupportingSurfaceType
	{
		Flat,
		Concave,
		Convex,
	};

	public SupportingSurfaceType surfaceType;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
