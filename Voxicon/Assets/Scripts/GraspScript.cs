using UnityEngine;
using System.Collections;

public class GraspScript : MonoBehaviour {
	private Animator anim;
	private int grasper;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			grasper = 1;
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			grasper = 2;
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
			grasper = 3;
		} else if (Input.GetKeyDown (KeyCode.Alpha4)) {
			grasper = 4;
		} else if (Input.GetKeyDown (KeyCode.Space)) {
			grasper = 0;
		}
		anim.SetInteger ("grasp", grasper);
		Debug.Log (grasper);
	}
}
