using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Macros : MonoBehaviour {

	public Dictionary<String,String> commandMacros = new Dictionary<String, String>();

	// Use this for initialization
	void Start () {
		commandMacros.Add ("close(mug)", "put(lid,on(mug))");
		commandMacros.Add ("stack(blocks)", "put(block1,on(block3));put(block2,on(block1));bind(block1,block2,block3,as(\"stack\"))");
		commandMacros.Add ("stack(apple,plate,mug)", "put(apple,on(plate));put(plate,on(mug))");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
