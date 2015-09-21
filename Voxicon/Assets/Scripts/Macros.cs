using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Macros : MonoBehaviour {

	public Dictionary<String,String> commandMacros = new Dictionary<String, String>();

	// Use this for initialization
	void Start () {
		commandMacros.Add ("close(mug)", "put(lid,on(mug))");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
