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
	public enum Role {
		Planner,
		Affector
	}
	public Role role;

	public enum Alignment {
		Left,
		Right
	}
	public Alignment alignment;

	public String outputLabel;
	public String outputString;
	public int outputHeight = 25;
	public Rect outputRect = new Rect();

	void Start() {
		if (alignment == Alignment.Left) {
			outputRect = new Rect (5, outputRect.y, 365, outputHeight);
		}
		else if (alignment == Alignment.Right) {
			outputRect = new Rect (Screen.width - 370, outputRect.y, 365, outputHeight);
		}
	}

	void Update() {
	}

	void OnGUI() {
		GUILayout.BeginArea (outputRect);
		GUILayout.BeginHorizontal();
		GUILayout.Label(outputLabel+":");
		outputString = GUILayout.TextArea(outputString, GUILayout.Width(300), GUILayout.ExpandHeight (false));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea();
	}
}

public static class OutputHelper {
	public static void PrintOutput(OutputController.Role role, String str) {
		OutputController[] outputs;
		outputs = GameObject.Find ("IOController").GetComponents<OutputController>();

		foreach (OutputController outputController in outputs) {
			if (outputController.role == role) {
				outputController.outputString = str;
			}
		}
	}
}
