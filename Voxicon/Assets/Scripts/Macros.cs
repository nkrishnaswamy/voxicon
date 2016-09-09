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
		commandMacros.Add ("build(staircase)", "put(green(block),left(black(block)));put(red(block),right(black(block)));put(yellow(block),on(red(block)));put(blue(block),on(black(block)));put(brown(block),on(yellow(block)))");
		commandMacros.Add ("build(pyramid)", "put(block1,left(block3));put(block2,right(block3));put(block4,behind(block3));put(block5,in_front(block3));put(block6,on(block3))");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
