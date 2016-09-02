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
		commandMacros.Add ("build(staircase)", "put(block1,right(block2));put(block3,left(block2));put(block4,on(block2));put(block5,left(block4));put(block6,on(block5))");
		commandMacros.Add ("build(pyramid)", "put(block1,left(block2));put(block3,right(block2));put(block4,behind(block2));put(block5,in_front(block2));put(block6,on(block2))");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
