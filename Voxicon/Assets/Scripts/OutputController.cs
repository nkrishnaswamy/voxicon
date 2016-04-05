using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Global;
using Satisfaction;

public class OutputController : MonoBehaviour {
	public String outputString;
	public int outputHeight = 25;
	public Rect outputRect;

	void Start() {
		outputRect = new Rect (Screen.width - 370, 5, 365, outputHeight);
	}

	void Update() {
	}

	void OnGUI() {
		GUILayout.BeginArea (outputRect);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Computer:");
		outputString = GUILayout.TextArea(outputString, GUILayout.Width(300), GUILayout.ExpandHeight (false));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea();
	}
}

public static class OutputHelper {
	public static void PrintOutput(String str) {
		((OutputController)(GameObject.Find ("IOController").GetComponent ("OutputController"))).outputString = str;
	}
}
