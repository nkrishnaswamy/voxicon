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
	private static extern void SelfHandshake(string port);

	[DllImport ("CommunicationsBridge")]
	private static extern IntPtr Process();

	[DllImport ("CommunicationsBridge")]
	private static extern bool ClosePort(string id);

	[DllImport ("CommunicationsBridge")]
	private static extern IntPtr PythonCall(string scriptsPath, string module, string function, string[] args, int numArgs);
	
	void Start () {
		port = PlayerPrefs.GetString ("Listener Port");

		if (port != "") {
			if (OpenPort (port)) {
				Debug.Log ("Listening on port " + port);
				SelfHandshake (port);
			}
			else {
				Debug.Log ("Failed to open port " + port);
			}
		}
		else {
			Debug.Log ("No listener port specified.  Skipping interface startup.");
		}
	}

	void Update () {
		if (port == "") {
			return;
		}

		string input = Marshal.PtrToStringAuto (Process ());
		if (input != "") {
			Debug.Log (input);
			((InputController)(GameObject.Find ("IOController").GetComponent ("InputController"))).inputString = input.Trim();
			((InputController)(GameObject.Find ("IOController").GetComponent ("InputController"))).MessageReceived(input.Trim());
		}
	}

	public string NLParse(string input) {
		string[] args = new string[]{input};
		string result = Marshal.PtrToStringAuto(PythonCall (Application.dataPath + "/Externals/python/", "change_to_forms", "parse_sent", args, args.Length));
		Debug.Log (result);

		return result;
	}

	void OnDestroy () {
		if (port == "") {
			return;
		}

		ClosePort (port);
	}

	void OnApplicationQuit () {
		if (port == "") {
			return;
		}

		ClosePort (port);
	}
}
