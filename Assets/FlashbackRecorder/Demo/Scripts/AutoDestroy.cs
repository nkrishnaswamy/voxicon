/*
 * Flashback Video Recorder
 * AutoDestroy.cs
 * 
 * Destroys the object if it falls below the specified height.
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */


using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {

	public float m_destroyHeight = -10;

	// Check the current y position, and if the object is below the specified height, destory it
	void Update () {
		if (transform.position.y < m_destroyHeight)
			DestroyImmediate (gameObject);
	}
		
}
