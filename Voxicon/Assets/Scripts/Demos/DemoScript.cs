using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.IO;

using Global;

public class DemoScript : MonoBehaviour {

	[HideInInspector]
	public bool moveLogged;

	[HideInInspector]
	public float logTimer;

	protected bool log;
	StreamWriter logFile;

	public Dictionary<string, Vector3> defaultState = new Dictionary<string, Vector3>();

	// Use this for initialization
	public void Start () {
		log = (PlayerPrefs.GetInt ("Make Logs") == 1);

		// log default state
		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject o in allObjects) {
			if (o.GetComponent<Voxeme> () != null) {
				if (o.GetComponent<Voxeme> ().enabled) {
					//Debug.Log (o.name);
					defaultState.Add (o.name, o.transform.position);
				}
			}
		}
	}

	// Update is called once per frame
	public void Update () {
		logTimer += Time.deltaTime;
	}

	protected void OpenLog(String name) {
		if (!log) {
			return;
		}

		string dateTime = DateTime.Now.ToString ("yyyy-MM-dd-HHmmss");
		logFile = new StreamWriter (name + @"-" + dateTime + @".txt");
	}

	protected void Log (string content, bool satisfied) {
		if (!log) {
			return;
		}

		if (!moveLogged) {
			logFile.WriteLine(string.Format("{0}\tMove: {1}",logTimer.ToString(),content));
			if (satisfied) {
				logFile.WriteLine(string.Format("{0}\t{1}",logTimer.ToString(),"Response: Agreement"));
			}
			else {
				logFile.WriteLine(string.Format("{0}\t{1}",logTimer.ToString(),"Response: Disagreement"));
			}
			moveLogged = true;
		}
	}

	protected void CloseLog() {
		if (!log) {
			return;
		}

		logFile.Close ();
	}
}

