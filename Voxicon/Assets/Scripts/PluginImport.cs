using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

using Global;

public class PluginImport : MonoBehaviour {
	// port definitions
	const int port = 7777;

	// Make our calls from the Plugin
	[DllImport ("CommunicationsBridge")]
	private static extern bool OpenPort(int id);
	
	void Start () {
		if (OpenPort (port)) {
			Debug.Log ("Listening on port " + Helper.IntToString (port));
		}
		else {
			Debug.Log ("Failed to open port " + Helper.IntToString (port));
		}
	}

	void Update () {
	}
}
