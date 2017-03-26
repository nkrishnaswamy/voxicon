using UnityEngine;
using System.Collections;

public class Ball : OldEntityClass {

	// Use this for initialization
	public override void Start () {
		base.Start();
		gameObject.AddComponent <Spin>();
		gameObject.AddComponent <Roll>();
		gameObject.AddComponent <Slide>();
		gameObject.AddComponent <Cross>();
		gameObject.AddComponent <Reach>();

		ContactType = SurfaceContactTypes.SurfaceContactType.Point;
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}
}
