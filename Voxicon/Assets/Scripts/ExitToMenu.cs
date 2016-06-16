using UnityEngine;
using System.Collections;

public class ExitToMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI () {
		if (GUI.Button (new Rect (10, Screen.height - 30, 100, 20), "Exit to Menu")) {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("VoxiconMenu");
			return;
		}
	}
}
