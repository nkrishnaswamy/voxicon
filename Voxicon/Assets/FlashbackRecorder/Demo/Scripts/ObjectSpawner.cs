/*
 * Flashback Video Recorder
 * ObjectSpawner.cs
 * 
 * 
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */

using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour {

	public float m_SpawnRadius = 5f;
	public Vector3 m_SpawnPoint = new Vector3 (0, 20, 4);
	public float m_WaitTimeMin = 0.3f;
	public float m_WaitTimeMax = 0.8f;
	public GameObject m_ObjectToSpawn;

	// Use this for initialization
	void Start () {
		//Start spawning physics objects
		StartCoroutine (SpawnObject());
	}


	//Repeatedly creates a new object above the platform
	public IEnumerator SpawnObject(){
		if (m_ObjectToSpawn != null) {
			Vector2 randomOffset = m_SpawnRadius * Random.insideUnitCircle;
			Vector3 position = m_SpawnPoint + new Vector3 (randomOffset.x, 0, randomOffset.y);
			GameObject ball = (GameObject)Instantiate (m_ObjectToSpawn, position, Quaternion.identity);
			Rigidbody rb = ball.GetComponent<Rigidbody> ();
			rb.AddForce (2.0f * Random.insideUnitSphere);

			yield return new WaitForSeconds (Random.Range (m_WaitTimeMin, m_WaitTimeMax));
			StartCoroutine (SpawnObject ());
		}
	}
}
