﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModalWindowEventArgs : EventArgs {

	public int WindowID { get; set; }

	public ModalWindowEventArgs(int windowID)
	{
		this.WindowID = windowID;
	}
}

public class ModalWindowManager : MonoBehaviour {

	public Dictionary<int, ModalWindow> windowManager = new Dictionary<int, ModalWindow>();

	public event EventHandler NewModalWindow;

	public void OnNewModalWindow(object sender, EventArgs e)
	{
		if (NewModalWindow != null)
		{
			NewModalWindow(this, e);
		}
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void RegisterWindow(ModalWindow window) {
		Debug.Log (string.Format("Register {0}:{1}",this,window.id));
		windowManager.Add(window.id,window);
	}

	public void UnregisterWindow(ModalWindow window) {
		Debug.Log (string.Format("Unregister {0}:{1}",this,window.id));
		windowManager.Remove(window.id);
	}
}
