using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModalWindowManager : MonoBehaviour {

	public Dictionary<int, ModalWindow> windowManager = new Dictionary<int, ModalWindow>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void RegisterWindow(ModalWindow window) {
		windowManager.Add(window.id,window);
	}

	public void UnregisterWindow(ModalWindow window) {
		windowManager.Remove(window.id);
	}
}
