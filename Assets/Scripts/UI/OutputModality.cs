﻿using UnityEngine;
using System.Collections;

public class OutputModality : FontManager {

	public enum Modality {
		Linguistic = 1,
		Gestural = (1 << 1)
	}
	public Modality modality = (Modality.Linguistic | Modality.Gestural);

	public int fontSize = 12;

	GUIStyle buttonStyle = new GUIStyle ("Button");

	float fontSizeModifier;

	Help help;

	// Use this for initialization
	void Start () {
		buttonStyle = new GUIStyle ("Button");
		fontSizeModifier = (int)(fontSize / defaultFontSize);
		buttonStyle.fontSize = fontSize;

		help = GameObject.Find ("Help").GetComponent<Help> ();
	}
	
	// Update is called once per frame
	void Update () {
		if ((int)(modality & Modality.Linguistic) == 0) {
			OutputHelper.PrintOutput (OutputController.Role.Planner, "");
		}
	}

	protected void OnGUI () {
		string buttonText = ((int)(modality & Modality.Linguistic) == 1) ? "Language Off" : "Language On";

		float buttonWidth = buttonStyle.CalcSize(new GUIContent (buttonText)).x + (14*fontSizeModifier);

		if (GUI.Button (new Rect (Screen.width - buttonWidth - 12, Screen.height - ((10 + (int)(20*help.FontSizeModifier)) + (5 + (int)(20*fontSizeModifier))),
			buttonWidth, 20*fontSizeModifier), buttonText, buttonStyle)) {
			modality ^= Modality.Linguistic;
		}
	}
}
