using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

using Global;

public class PluginImport : MonoBehaviour {
	// port definitions
	string port = "";

	// Make our calls from the Plugin
	[DllImport ("CommunicationsBridge")]
	private static extern bool OpenPort(string id);

	[DllImport ("CommunicationsBridge")]
	private static extern IntPtr Process();

	[DllImport ("CommunicationsBridge")]
	private static extern bool ClosePort(string id);
	
	void Start () {
		port = PlayerPrefs.GetString ("Listener Port");

		if (OpenPort (port)) {
			Debug.Log ("Listening on port " + port);
		}
		else {
			Debug.Log ("Failed to open port " + port);
		}
	}

	void Update () {
		string input = Marshal.PtrToStringAuto (Process ());
		if (input != "") {
			Debug.Log (input);
			((InputController)(GameObject.Find ("InputController").GetComponent ("InputController"))).inputString = input;
		}
	}

	void OnApplicationQuit () {
		ClosePort (port);
	}
}
