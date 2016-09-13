using UnityEngine;
using System.Collections;

public class OutputModality : MonoBehaviour {

	public enum Modality {
		Linguistic = 1,
		Gestural = (1 << 1)
	}
	public Modality modality = (Modality.Linguistic | Modality.Gestural);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if ((int)(modality & Modality.Linguistic) == 0) {
			OutputHelper.PrintOutput (OutputController.Role.Planner, "");
		}
	}

	protected void OnGUI () {
		string buttonText = ((int)(modality & Modality.Linguistic) == 1) ? "Language Off" : "Language On";

		float buttonWidth = GUI.skin.label.CalcSize (new GUIContent (buttonText)).x + 14;

		if (GUI.Button (new Rect (Screen.width - buttonWidth - 12, Screen.height - 60, buttonWidth, 22), buttonText)) {
			modality ^= Modality.Linguistic;
		}
	}
}
