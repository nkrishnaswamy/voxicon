using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTypes;

public class Spin : Behavior {
		
	public float speed = 200.0f;
	
	// Use this for initialization
	public override void Start () {
		base.Start ();

		arity = 1;
		StopOnCollide = false;
		formula = "spin";
		type = BehaviorType.Spin;
	}
	
	// Update is called once per frame
	void Update () {
		if (entity.CurrentBehaviorTypeProperty != type)
			return;

		if (Pause)
			return;

		entity.CurrentBehavior = this;

		transform.eulerAngles = new Vector3(0.0f,(float)transform.eulerAngles.y+(speed*Time.deltaTime),0.0f);
	}

	public override void SetNewBehavior(List<string> args) {
		base.SetNewBehavior (args);
	}
}

