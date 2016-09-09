using UnityEngine;
using System.Collections;

using Global;

public class DemoScript : MonoBehaviour {

	enum DemoStep {
		Step0,
		Step1A,
		Step1B,
		Step1C,
		Step2A,
		Step2B,
		Step2C,
		Step3A,
		Step3B,
		Step3C,
		Step4A,
		Step4B,
		Step4C,
		Step5A,
		Step5B,
		Step5C,
		Step6
	}

	GameObject Wilson;
	Animator animator;

	DemoStep currentStep;

	// Use this for initialization
	void Start () {
		Wilson = GameObject.Find ("Wilson");
		animator = Wilson.GetComponent<Animator> ();

		currentStep = DemoStep.Step0;
	}
	
	// Update is called once per frame
	void Update () {
		if (currentStep == DemoStep.Step0) {
			Rest ();
		}

		if (currentStep == DemoStep.Step1A) {
			PointAt (GameObject.Find ("block4"));
		}

		if (currentStep == DemoStep.Step1B) {
			PointAt (GameObject.Find ("block3"));
		}

		if (currentStep == DemoStep.Step1C) {
			PushTogether ();
		}

		if (currentStep == DemoStep.Step2A) {
			PointAt (GameObject.Find ("block6"));
		}

		if (currentStep == DemoStep.Step2B) {
			PointAt (GameObject.Find ("block3"));
		}

		if (currentStep == DemoStep.Step2C) {
			PushTogether ();
		}

		if (currentStep == DemoStep.Step3A) {
			PointAt (GameObject.Find ("block5"));
		}

		if (currentStep == DemoStep.Step3B) {
			PointAt (GameObject.Find ("block6"));
		}

		if (currentStep == DemoStep.Step3C) {
			Claw (Vector3.zero,Vector3.one);
		}

		if (currentStep == DemoStep.Step4A) {
			PointAt (GameObject.Find ("block2"));
		}

		if (currentStep == DemoStep.Step4B) {
			PointAt (GameObject.Find ("block3"));
		}

		if (currentStep == DemoStep.Step4C) {
			Claw (Vector3.zero,Vector3.one);
		}

		if (currentStep == DemoStep.Step5A) {
			PointAt (GameObject.Find ("block1"));
		}

		if (currentStep == DemoStep.Step5B) {
			PointAt (GameObject.Find ("block5"));
		}

		if (currentStep == DemoStep.Step5C) {
			Claw (Vector3.zero,Vector3.one);
		}

		if (currentStep == DemoStep.Step6) {
			Rest ();
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			currentStep = (DemoStep)((int)currentStep + 1);
		}
	}

	void Rest() {
		GraspScript graspController = Wilson.GetComponent<GraspScript> ();
		IKControl ikControl = Wilson.GetComponent<IKControl> ();

		graspController.grasper = (int)Gestures.HandPose.Neutral;

		if (ikControl != null) {
			ikControl.leftHandObj.transform.position = graspController.leftDefaultPosition;
			ikControl.rightHandObj.transform.position = graspController.rightDefaultPosition;
		}
	}

	void PointAt(GameObject obj) {
		GameObject leftGrasper = animator.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
		GameObject rightGrasper = animator.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
		GameObject grasper;
		GraspScript graspController = Wilson.GetComponent<GraspScript> ();

		// find bounds corner closest to grasper
		Bounds bounds = Helper.GetObjectWorldSize (obj);

		// which hand is closer?
		float leftToGoalDist = (leftGrasper.transform.position - bounds.ClosestPoint (leftGrasper.transform.position)).magnitude;
		float rightToGoalDist = (rightGrasper.transform.position - bounds.ClosestPoint (rightGrasper.transform.position)).magnitude;

		if (leftToGoalDist < rightToGoalDist) {
			grasper = leftGrasper;
			graspController.grasper = (int)Gestures.HandPose.LeftPoint;
		}
		else {
			grasper = rightGrasper;
			graspController.grasper = (int)Gestures.HandPose.RightPoint;
		}

		IKControl ikControl = Wilson.GetComponent<IKControl> ();
		if (ikControl != null) {
			Vector3 target = new Vector3 (bounds.center.x, bounds.center.y-0.2f, bounds.center.z+0.3f);
			if (grasper == leftGrasper) {
				ikControl.leftHandObj.transform.position = target;
				ikControl.rightHandObj.transform.position = graspController.rightDefaultPosition;
			}
			else {
				ikControl.leftHandObj.transform.position = graspController.leftDefaultPosition;
				ikControl.rightHandObj.transform.position = target;
			}
		}
	}

	void PushTogether() {
		GameObject leftGrasper = animator.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
		GameObject rightGrasper = animator.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
		GameObject grasper;
		IKControl ikControl = Wilson.GetComponent<IKControl> ();

		Wilson.GetComponent<GraspScript> ().grasper = (int)Gestures.HandPose.Neutral;

		if (ikControl != null) {
			ikControl.leftHandObj.transform.position = new Vector3(0.5f,2.5f,0.0f);
			ikControl.rightHandObj.transform.position = new Vector3(-0.5f,2.5f,0.0f);
		}
	}

	void Claw(Vector3 from, Vector3 to) {
		GameObject leftGrasper = animator.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
		GameObject rightGrasper = animator.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
		GameObject grasper;

		Wilson.GetComponent<GraspScript> ().grasper = (int)Gestures.HandPose.RightClaw;
	}
}
