using UnityEngine;
using System.Collections;

public class Box : OldEntityClass {
	
	// Use this for initialization
	public override void Start () {
		base.Start ();
		gameObject.AddComponent <Spin>();
		gameObject.AddComponent <Slide>();
		gameObject.AddComponent <Cross>();
		gameObject.AddComponent <Reach>();

		ContactType = SurfaceContactTypes.SurfaceContactType.Area;
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}
}